﻿using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: InternalsVisibleTo("GrooveSUPatcher")]
//[assembly: HG.Reflection.SearchableAttribute.OptIn]