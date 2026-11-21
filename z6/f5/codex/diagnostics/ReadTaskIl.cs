using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

// Static PE reader only: never loads or executes third-party task code.
// Build in an ignored scratch directory with the already installed .NET SDK.
if (args.SequenceEqual(new[] { "--environment" }))
{
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
    {
        ProcessArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
        OSArchitecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString(),
        Is64BitProcess = Environment.Is64BitProcess,
        Process_PROCESSOR_ARCHITECTURE = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"),
        Machine_PROCESSOR_ARCHITECTURE = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine),
        User_PROCESSOR_ARCHITECTURE = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.User),
        Process_PROCESSOR_ARCHITEW6432 = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")
    }));
    return;
}
if (args.Length < 2) throw new ArgumentException("assembly-path type-name-filter [more-filters]");
using var stream = File.OpenRead(args[0]);
using var pe = new PEReader(stream);
var reader = pe.GetMetadataReader();
var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
    .Where(field => field.FieldType == typeof(OpCode))
    .Select(field => (OpCode)field.GetValue(null)!)
    .ToDictionary(op => unchecked((ushort)op.Value));

string Name(EntityHandle handle) => handle.Kind switch
{
    HandleKind.TypeReference => TypeRefName(reader.GetTypeReference((TypeReferenceHandle)handle)),
    HandleKind.TypeDefinition => TypeDefName(reader.GetTypeDefinition((TypeDefinitionHandle)handle)),
    HandleKind.MemberReference => MemberName(reader.GetMemberReference((MemberReferenceHandle)handle)),
    HandleKind.MethodDefinition => MethodName(reader.GetMethodDefinition((MethodDefinitionHandle)handle)),
    HandleKind.FieldDefinition => reader.GetString(reader.GetFieldDefinition((FieldDefinitionHandle)handle).Name),
    HandleKind.MethodSpecification => Name(reader.GetMethodSpecification((MethodSpecificationHandle)handle).Method),
    _ => $"{handle.Kind}:0x{MetadataTokens.GetToken(handle):X8}"
};
string TypeRefName(TypeReference type) => reader.GetString(type.Namespace) + "." + reader.GetString(type.Name);
string TypeDefName(TypeDefinition type) => reader.GetString(type.Namespace) + "." + reader.GetString(type.Name);
string MemberName(MemberReference member) => Name(member.Parent) + "::" + reader.GetString(member.Name);
string MethodName(MethodDefinition method) => Name(method.GetDeclaringType()) + "::" + reader.GetString(method.Name);

Console.WriteLine("Static IL; no target assembly execution");
Console.WriteLine("assembly=" + Path.GetFileName(args[0]));
Console.WriteLine("sha256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0]))).ToLowerInvariant());
foreach (var typeHandle in reader.TypeDefinitions)
{
    var type = reader.GetTypeDefinition(typeHandle);
    string typeName = TypeDefName(type);
    if (!args.Skip(1).Any(filter => typeName.Contains(filter, StringComparison.Ordinal))) continue;
    Console.WriteLine("TYPE " + typeName);
    foreach (var methodHandle in type.GetMethods())
    {
        var method = reader.GetMethodDefinition(methodHandle);
        Console.WriteLine("METHOD " + reader.GetString(method.Name));
        if (method.RelativeVirtualAddress == 0) continue;
        var body = pe.GetMethodBody(method.RelativeVirtualAddress);
        var bytes = body.GetILBytes()!;
        int pos = 0;
        while (pos < bytes.Length)
        {
            int start = pos;
            ushort key = bytes[pos++];
            if (key == 0xFE) key = (ushort)(0xFE00 | bytes[pos++]);
            var op = opcodes[key];
            string operand = "";
            switch (op.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.InlineString:
                    int stringToken = BitConverter.ToInt32(bytes, pos); pos += 4;
                    operand = System.Text.Json.JsonSerializer.Serialize(reader.GetUserString(MetadataTokens.UserStringHandle(stringToken & 0xFFFFFF)));
                    break;
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                    int token = BitConverter.ToInt32(bytes, pos); pos += 4;
                    operand = Name(MetadataTokens.EntityHandle(token));
                    break;
                case OperandType.ShortInlineBrTarget:
                    operand = $"IL_{pos + 1 + (sbyte)bytes[pos]:X4}"; pos++; break;
                case OperandType.InlineBrTarget:
                    operand = $"IL_{pos + 4 + BitConverter.ToInt32(bytes, pos):X4}"; pos += 4; break;
                case OperandType.ShortInlineI: operand = ((sbyte)bytes[pos++]).ToString(); break;
                case OperandType.ShortInlineVar: operand = bytes[pos++].ToString(); break;
                case OperandType.InlineVar: operand = BitConverter.ToUInt16(bytes, pos).ToString(); pos += 2; break;
                case OperandType.InlineI: operand = BitConverter.ToInt32(bytes, pos).ToString(); pos += 4; break;
                case OperandType.InlineI8: operand = BitConverter.ToInt64(bytes, pos).ToString(); pos += 8; break;
                case OperandType.ShortInlineR: operand = BitConverter.ToSingle(bytes, pos).ToString(); pos += 4; break;
                case OperandType.InlineR: operand = BitConverter.ToDouble(bytes, pos).ToString(); pos += 8; break;
                case OperandType.InlineSig: operand = "signature"; pos += 4; break;
                case OperandType.InlineSwitch:
                    int count = BitConverter.ToInt32(bytes, pos); pos += 4;
                    operand = $"targets={count}"; pos += count * 4; break;
                default: throw new InvalidDataException(op.OperandType.ToString());
            }
            Console.WriteLine($"  IL_{start:X4}: {op.Name} {operand}".TrimEnd());
        }
    }
}
