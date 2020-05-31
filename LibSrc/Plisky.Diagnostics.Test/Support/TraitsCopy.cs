using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plisky.Diagnostics.Copy {

    // DO NOT MODIFY.....

#pragma warning disable IDE1006

    /// <summary>
    /// THIS IS A COPY - this is not the original and should not be modified.  This is used because Plisky.Plumbing depends on Plisky.Diagnostics
    /// but diags wants to use an element of plumbing for consistancy.
    /// </summary>
    public class Traits {
        /// <summary>
        /// Describes the age of the test, Exploratory, Fresh, Regression, Interface
        /// </summary>
        public const string Age = "Age";

        /// <summary>
        /// Describes the stlye of the test, such as its dependencyt on deployments or database.  Examples are Integration, Unit, Smoke.
        /// </summary>

        public const string Style = "Style";


        /// <summary>
        /// Fresh tests are new, hot off the press.  They are for new features of code and therefore less stable than other types of test.
        /// </summary>
        public const string Fresh = "Fresh";

        /// <summary>
        /// Regression tests define the code that is well established, these should not be failing.
        /// </summary>
        public const string Regression = "Regression";

        /// <summary>
        /// Style - Exploratory tests relate to bleeding edge or features that require environmental configuration, they are often skipped during build testnig
        /// </summary>
        public const string Exploratory = "Exploratory";

        /// <summary>
        /// Age - Interface tests relate to released, public, interfaces.  They should not break for any reason.
        /// </summary>
        public const string Interface = "Interface";

        /// <summary>
        /// Style - Integration tests involve connectivity to other elements of the sytstem (such as databases and the like)
        /// </summary>
        public const string Integration = "Integration";

        /// <summary>
        /// Style - Unit tests involve small isolated lements of the code 
        /// </summary>
        public const string Unit = "Unit";

        /// <summary>
        /// Style - Developer Tests are designed to run on development machines only, not on build servers.  This is because they either
        /// use specific configuration for dealing with private elements of a class or they depend on settings and data stored locally.
        /// </summary>
        public const string Developer = "Dev";

        /// <summary>
        /// Style - Smoke tests are integrated, but leightweight non distructive - suitable for production tests.
        /// </summary>
        public const string Smoke = "Smoke";

        /// <summary>
        /// Style - Live bug tests are specific examples that occured in prodution and have been isolated to test cases in the code
        /// this is to ensure that we dont regress these in future and learn from our mistakes.
        /// </summary>
        public const string LiveBug = "Bug";
    }

#pragma warning restore IDE1006 // Naming Styles
}
