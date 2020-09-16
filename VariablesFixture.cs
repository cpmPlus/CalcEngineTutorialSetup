using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.Vtrin;
using ABB.Vtrin.Controls;
using ABB.Vtrin.Drivers;

namespace CalcEngineTutorialSetup
{
    class VariablesFixture
    {
        private cDriverSkeleton driver;

        public VariablesFixture(cDriverSkeleton _driver)
        {
            driver = _driver;
        }

        public void Setup()
        {
            addVariable("CalcTutorial_A", 10.0, "Calc Engine tutorial");
            addVariable("CalcTutorial_B", 20.0, "Calc Engine tutorial");
            addVariable("CalcTutorial_C", 0.0,  "Calc Engine tutorial");
        }

        private void addVariable(string name, object value, string description)
        {
            var variableCache = driver.Classes["Variable"].Instances;

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
        }

        public void Cleanup()
        { }
    }
}
