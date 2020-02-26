using Microsoft.VisualStudio.TestTools.UnitTesting;
using HC_Lib.Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Maple.Tests
{
    [TestClass()]
    public class MapleMatrixTests
    {
        [TestMethod()]
        public void LPrintConstructor()
        {
            var SampleInput = "Matrix(3,3,{(1, 1) = 1, (2, 2) = 1, (3, 3) = 1},datatype = anything,storage = rectangular,order = Fortran_order,shape = [])";
            MapleMatrix Matrix = new MapleMatrix(SampleInput);

            Assert.AreEqual(3, Matrix.Rows);
            Assert.AreEqual(3, Matrix.Columns);
            Assert.AreEqual("1", Matrix.Values[0][0]);
            Assert.AreEqual("1", Matrix.Values[1][1]);
            Assert.AreEqual("1", Matrix.Values[2][2]);
            Assert.AreEqual("0", Matrix.Values[2][1]);
        }
    }
}