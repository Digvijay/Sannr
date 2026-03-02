using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sannr;

namespace Sannr.AspNetCore
{
    /// <summary>
    /// Global registry for Sannr validators.
    /// Provides high-performance O(1) lookup for compile-time generated validators.
    /// </summary>
    public static class SannrValidatorRegistry
    {
        private static readonly ConcurrentDictionary<Type, Func<object, Task<ValidationResult>>> _validators = new();

        /// <summary>
        /// Registers a validator for a given type.
        /// Usually called by source-generated code.
        /// </summary>
        /// <typeparam name="T">The type to validate.</typeparam>
        /// <param name="validator">The validation delegate.</param>
        public static void Register<T>(Func<T, Task<ValidationResult>> validator)
        {
            _validators[typeof(T)] = async (obj) => await validator((T)obj);
        }

        /// <summary>
        /// Attempts to get a validator for the specified type.
        /// </summary>
        /// <param name="type">The type to validate.</param>
        /// <param name="validator">The found validator, if any.</param>
        /// <returns>True if a validator was found.</returns>
        public static bool TryGetValidator(Type type, out Func<object, Task<ValidationResult>>? validator)
        {
            return _validators.TryGetValue(type, out validator);
        }

        /// <summary>
        /// Validates an object instance using the registered validator.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <returns>The validation result.</returns>
        public static async Task<ValidationResult> ValidateAsync(object instance)
        {
            if (instance == null) return ValidationResult.Success();

            if (TryGetValidator(instance.GetType(), out var validator) && validator != null)
            {
                return await validator(instance);
            }

            return ValidationResult.Success();
        }
    }
}
