
#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.ComponentModel;
#pragma warning restore IDE0130 // Namespace does not match folder structure

namespace System.Runtime.CompilerServices;

#if !NET5_0_OR_GREATER

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit;

#endif // !NET5_0_OR_GREATER
