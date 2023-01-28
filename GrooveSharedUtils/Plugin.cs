/*using BepInEx;
using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using MonoMod.Cil;
using BepInEx.Bootstrap;
using Mono.Cecil.Cil;
using UnityEngine;
using System.Collections.Generic;
using RoR2;
using R2API.ScriptableObjects;
using GrooveSharedUtils.Frames;
using R2API;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;
using GrooveSharedUtils.Interfaces;

namespace GrooveSharedUtils
{
    internal class Plugin : BaseModPlugin<Plugin>
    {
               
        public override string PLUGIN_ModName => "GrooveSharedUtils";
        public override string PLUGIN_AuthorName => "groovesalad";
        public override string PLUGIN_VersionNumber => "1.0.0";
        public override string[] PLUGIN_HardDependencyStrings => GSUtil.Array( Common.Dependencies.R2API );

        public override void BeginModInit()
        {
        }
        public override void BeginCollectContent(AssetStream sasset)
        {
        }
    }
}*/
/*using System;
using RoR2;

public class ClassA<TClass> where TClass : ClassA<TClass> 
{
    public TClass ClassAMethod() => this as TClass;
} 
public abstract class ClassB<TClass, T> : ClassA<TClass> where TClass : ClassB<TClass, T> where T : ItemDef 
{
    public TClass ClassBMethod() => this as TClass;
}
public abstract class InheritedClassB : ClassB<InheritedClassB, ItemDef> { }
public class GenericInheritedClassB<T> : ClassB<GenericInheritedClassB<T>, T> where T : ItemDef { } 
public static class TestClass
{
    public static void Test()
    {
        new InheritedClassB().ClassAMethod().ClassAMethod();
        new InheritedClassB().ClassBMethod();
        new GenericInheritedClassB<ItemDef>().ClassBMethod();
    }
}*/
