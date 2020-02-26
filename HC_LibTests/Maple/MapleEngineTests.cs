using Microsoft.VisualStudio.TestTools.UnitTesting;
using HC_Lib.Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace HC_Lib.Maple.Tests
{
    [TestClass()]
    public class MapleEngineTests
    {
        const string path = @"C:\Program Files\Maple 2019\bin.X86_64_WINDOWS\cmaple.exe";//modify to cmaple before running tests.
        
        [TestMethod()]
        public void MapleEngineTest()
        {
            var engine = new MapleEngine(path);

            var defPath = engine.GetType().GetField("MaplePath", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(engine);
            Assert.IsNotNull(defPath);
            Assert.AreEqual(path, defPath);
        }

        [TestMethod()]
        public void OpenTest()
        {
            var engine = new MapleEngine(path);
            try
            {
                Assert.IsNull(engine.GetType().GetField("MapleProcess", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(engine));
                engine.Open();
                Assert.IsNotNull(engine.GetType().GetField("MapleProcess", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(engine));
                engine.Close();
            } catch (Win32Exception)
            {
                Assert.Fail("Invalid Path.");
            }
        }

        [TestMethod()]
        public void SimplifyTest()
        {
            var engine = new MapleEngine(path);
            try
            {
                engine.Open();
                var simplified = engine.Simplify("15*x + 7*x").Result;
                Assert.AreEqual("22*x", simplified.Replace("\r\n", string.Empty));
                engine.Close();
            }
            catch (Win32Exception)
            {
                Assert.Fail("Invalid Path.");
            }
        }
    }
}