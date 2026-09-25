using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Sannr;

namespace Sannr.AspNetCore
{
    /// <summary>
    /// Obsolete forwarder to <see cref="global::Sannr.SannrValidatorRegistry"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type used to maintain its own, separate dictionary of validators. Nothing ever wrote to
    /// it: the source generator registers into <see cref="global::Sannr.SannrValidatorRegistry"/>.
    /// Because this type lives in the <c>Sannr.AspNetCore</c> namespace, it shadowed the real
    /// registry at every unqualified call site inside this assembly, so the endpoint filter looked
    /// up validators in a dictionary that was permanently empty, found none, and let every request
    /// through unvalidated.
    /// </para>
    /// <para>
    /// It is now a thin forwarder over the real registry, so the two can no longer disagree, and any
    /// existing caller keeps working against the same storage the generator uses.
    /// </para>
    /// </remarks>
    [Obsolete("Use Sannr.SannrValidatorRegistry. This type forwards to it and will be removed in a future major version.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SannrValidatorRegistry
    {
        /// <summary>
        /// Registers a validator for a given type in the shared registry.
        /// </summary>
        /// <typeparam name="T">The type to validate.</typeparam>
        /// <param name="validator">The validation delegate.</param>
        public static void Register<T>(Func<T, Task<ValidationResult>> validator)
        {
            ArgumentNullException.ThrowIfNull(validator);

            global::Sannr.SannrValidatorRegistry.Register<T>(
                context => validator((T)context.ObjectInstance));
        }

        /// <summary>
        /// Attempts to get a validator for the specified type from the shared registry.
        /// </summary>
        /// <param name="type">The type to validate.</param>
        /// <param name="validator">The found validator, if any.</param>
        /// <returns><see langword="true"/> if a validator was found.</returns>
        public static bool TryGetValidator(Type type, out Func<object, Task<ValidationResult>>? validator)
        {
            if (global::Sannr.SannrValidatorRegistry.TryGetValidator(type, out var inner) && inner != null)
            {
                validator = instance => inner(new SannrValidationContext(instance));
                return true;
            }

            validator = null;
            return false;
        }

        /// <summary>
        /// Validates an object instance using the registered validator.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <returns>The validation result.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no validator is registered for the instance's type. This previously returned
        /// success, which made a missing validator indistinguishable from valid input.
        /// </exception>
        public static async Task<ValidationResult> ValidateAsync(object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            var type = instance.GetType();
            if (!global::Sannr.SannrValidatorRegistry.TryGetValidator(type, out var validator) || validator == null)
            {
                throw new InvalidOperationException(
                    $"No Sannr validator is registered for '{type.FullName}'. Returning success here " +
                    "would make a missing validator indistinguishable from valid input, so this " +
                    "throws instead. Check that the model is annotated, that the Sannr generator ran " +
                    "for the declaring assembly, and that the generated registration runs at startup.");
            }

            return await validator(new SannrValidationContext(instance)).ConfigureAwait(false);
        }
    }
}
