using HarmonyLib;
using System;

namespace CSLMusicMod.Helpers
{
    /// <summary>
    /// Helpers to improve QOL with reflection
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets a private field of an object
        /// </summary>
        /// <returns>The private field.</returns>
        /// <param name="instance">The object</param>
        /// <param name="name">Name of the private field</param>
        /// <typeparam name="T">Type of the private field</typeparam>
        public static T GetPrivateField<T>(object instance, string name) => Traverse.Create(instance).Field(name).GetValue<T>();
        /// <summary>
        /// Gets a private static field of a class
        /// </summary>
        /// <returns>The private static field.</returns>
        /// <param name="type">Class with the private field</param>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="T">Type of the field</typeparam>
        public static T GetPrivateStaticField<T>(Type type, string name) => Traverse.Create(type).Field(name).GetValue<T>();

        /// <summary>
        /// Sets a private field.
        /// </summary>
        /// <param name="instance">The object</param>
        /// <param name="name">Name of the private field</param>
        /// <param name="value">Value to be set</param>
        public static void SetPrivateField(object instance, string name, object value) => Traverse.Create(instance).Field(name).SetValue(value);

        /// <summary>
        /// Sets a private static field of a class.
        /// </summary>
        /// <param name="type">The class</param>
        /// <param name="name">Name of the private static field</param>
        /// <param name="value">Value to be set</param>
        public static void SetPrivateStaticField(Type type, string name, object value) => Traverse.Create(type).Field(name).SetValue(value);

        /// <summary>
        /// Invokes a private function.
        /// </summary>
        /// <returns>Return value of the procedure</returns>
        /// <param name="instance">Object with the private method</param>
        /// <param name="method">Name of the method</param>
        /// <param name="parameters">Parameters of the method</param>
        /// <typeparam name="T">Return type of the method</typeparam>
        public static T InvokePrivateMethod<T>(object instance, string method, params object[] parameters) => Traverse.Create(instance).Method(method, parameters).GetValue<T>();
        /// <summary>
        /// Invokes a private procedure.
        /// </summary>
        /// <param name="instance">Object with the private method.</param>
        /// <param name="method">Name of the method.</param>
        /// <param name="parameters">Parameters of the method.</param>
        public static void InvokePrivateVoidMethod(object instance, string method, params object[] parameters) => Traverse.Create(instance).Method(method, parameters).GetValue();
    }
}

