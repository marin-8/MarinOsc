
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics.CodeAnalysis;
#pragma warning restore IDE0130 // Namespace does not match folder structure

#if !NET7_0_OR_GREATER

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
internal sealed class SetsRequiredMembersAttribute : Attribute { }

#endif // !NET7_0_OR_GREATER
