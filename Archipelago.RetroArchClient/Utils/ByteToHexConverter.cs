using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Archipelago.RetroArchClient.Utils;

public class ByteToHexConverter : IYamlTypeConverter
{
	public bool Accepts(Type type)
		=> type == typeof(byte) || type == typeof(byte?);

	public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
	{
		var scalar = (Scalar)parser.Current!;

		if (scalar.Value == null)
		{
			throw new InvalidOperationException("Invalid YAML scalar value.");
		}

		parser.MoveNext();

		return Convert.ToByte(scalar.Value, 16);
	}

	public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
	{
		var hexValue = $"0x{Convert.ToByte(value):X2}";
		emitter.Emit(new Scalar(hexValue));
	}
}