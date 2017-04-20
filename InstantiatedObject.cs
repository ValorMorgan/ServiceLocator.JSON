using System;
using System.ComponentModel.DataAnnotations;

namespace ServiceLocator.JSON
{
    /// <summary>
    /// Representation of an instantiated object in the system.
    /// </summary>
    /// <remarks>
    /// Used as a means to keep a single instance of an object alive
    /// so that other areas of code do not need to initialize any new
    /// instances and may act upon the same set.  This is contradictive
    /// to the use of "GetSingleInstance" from the Resolver that creates
    /// an instance of the object but does NOT store the instance as this
    /// type of object.
    /// <note type="note">The InstantiatedObject uses DataAnnotations along with a
    /// validation check from the Registry to validate each incoming object.</note>
    /// </remarks>
    internal class InstantiatedObject : IDisposable
    {
        /// <summary>
        /// The Class Type of the Object.
        /// </summary>
        [Required(ErrorMessage = "The ClassType is required.")]
        public Type ClassType { get; set; }

        /// <summary>
        /// The Interface Type of the Object.
        /// </summary>
        [Required(ErrorMessage = "The InterfaceType is required.")]
        public Type InterfaceType { get; set; }

        /// <summary>
        /// The Object itself.
        /// </summary>
        [Required(ErrorMessage = "The Object is required.")]
        public dynamic TheObject { get; set; }

        public bool AllowMultiple { get; set; }

        public void Validate()
        {
            ValidationContext context = new ValidationContext(this);
            Validator.ValidateObject(this, context);

            if (!this.ClassType.IsClass)
                throw new ValidationException($"The {nameof(InstantiatedObject)} does not have a proper {nameof(this.ClassType)}: \"{this.ClassType.FullName}\".");
            if (!this.InterfaceType.IsInterface)
                throw new ValidationException($"The {nameof(InstantiatedObject)} does not have a proper {nameof(this.InterfaceType)}: \"{this.InterfaceType.FullName}\".");
        }

        /// <summary>
        /// Converts the information stored within this Class to a String format.
        /// </summary>
        /// <returns>The information stored within this Class to a String format.</returns>
        public override string ToString()
        {
            return string.Format("{1} - Name: {2}{0}Interface Type: {3}{0}Class Type: {4}",
                Environment.NewLine,
                nameof(InstantiatedObject),
                ClassType.Name ?? "NULL",
                InterfaceType.FullName ?? "NULL",
                ClassType.FullName ?? "NULL"
            );
        }

        /// <summary>
        /// Disposes the InstantiatedObject and calls the stored object's Dispose
        /// method if the object implements IDisposable.
        /// </summary>
        public void Dispose()
        {
            if (TheObject is IDisposable)
                TheObject.Dispose();
        }
    }
}
