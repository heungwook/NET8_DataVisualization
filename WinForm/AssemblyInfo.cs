using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using System.Runtime.InteropServices;
using System.Resources;
using System.Windows;

//
// General Information about an assembly is controlled through the following 
// set of properties. Change these attribute values to modify the information
// associated with an assembly.
//
//[assembly: InternalsVisibleTo("Orion.DataVisualization.Design, PublicKey=" + Orion.DataVisualization.Charting.AssemblyRef.SharedLibPublicKeyFull)]

// Permissions required
//[assembly: SecurityCritical]

#if VS_BUILD
[assembly: AssemblyVersion(Orion.DataVisualization.Charting.ThisAssembly.Version)]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("")]
//[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
#endif //VS_BUILD

//[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA900:AptcaAssembliesShouldBeReviewed", 
//    Justification = "We have APTCA signoff, for details please refer to SWI Track, Project ID 7972")]