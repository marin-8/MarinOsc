namespace MarinOsc1.Common.Internal;

internal static class BitConverter
{
	public static unsafe int SingleToInt32Bits (float value)
	{
		return *(int*)&value;
	}

	public static unsafe float Int32BitsToSingle (int value)
	{
		return *(float*)&value;
	}
}
