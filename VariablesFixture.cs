using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ABB.Vtrin;
using ABB.Vtrin.Controls;
using ABB.Vtrin.Drivers;

namespace CalcEngineTutorialSetup
{
    class VariablesFixture : IFixture
    {
        public void Setup()
        {
            createVariableFacade("CalcTutorial_A", 10.0, "Calc Engine tutorial");
            createVariableFacade("CalcTutorial_B", 20.0, "Calc Engine tutorial");
            createVariableFacade("CalcTutorial_C", 0.0, "Calc Engine tutorial");
        }

        private void createVariableFacade(string name, object value, string description)
        {
            var variable = addVariable(name, value, description);
            if (Context.Group != null)
            {
                addVariableAcl(variable);
            }
        }

        private cDbVariable addVariable(string name, object value, string description)
        {
            var variableCache = Context.Driver.Classes["Variable"].Instances;

            var variable = (cDbVariable)variableCache[name]?.BeginUpdate();
            if (variable == null)
            {
                variable = (cDbVariable)variableCache.Add();
            }

            variable["Name"] = name;
            variable["Description"] = description;
            variable.CommitChanges();

            var cv = variable.pCurrentValue.BeginUpdate();
            cv.Value = value;
            cv.CommitChanges();

            return variable;
        }

        private void removeVariables()
        {
            var variables = Context.Driver.Classes["Variable"].Instances.GetInstanceSet("Name LIKE 'CalcTutorial_*'");

            foreach (var variable in variables)
            {
                variable.Remove();
            }
        }

        private void addVariableAcl(cDbVariable variable)
        {
            Acl.CreateAcl(variable.Name, $"/Variable/{variable.Id}");
        }

        private void removeAclEntries()
        {
            Acl.RemoveAclEntries("CalcTutorial_*");
        }

        public void Cleanup()
        {
            removeAclEntries();
            removeVariables();
        }
    }
}
