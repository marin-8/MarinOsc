
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130 // Namespace does not match folder structure

#if !NET7_0_OR_GREATER

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
	public CompilerFeatureRequiredAttribute (string featureName)
	{
		FeatureName = featureName;
	}

	public string FeatureName { get; }
	public bool IsOptional { get; init; }

	public const string RefStructs = nameof(RefStructs);
	public const string RequiredMembers = nameof(RequiredMembers);
}

#endif // !NET7_0_OR_GREATER
