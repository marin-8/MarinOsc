
namespace MarinOsc.Common;

public enum OscTypeTag : byte
{
	Int32 = 105,      // i
	Float32 = 102,    // f
	OscString = 115,  // s
	//OscBlob = 98,     // b //

	//OscTimetag = 116, // t //
	True = 84,        // T
	False = 70,       // F
	Nil = 78,         // N
	//Infinitum = 73,   // I //
}
