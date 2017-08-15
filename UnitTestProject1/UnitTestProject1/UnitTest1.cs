using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var userName = "colin";
            var token = JsonWebToken.GenerateToken(userName);

            var userNameTmp = string.Empty;
            var result = JsonWebToken.ValidateToken(token, out userNameTmp);
            Assert.AreEqual(true, result);

            Assert.AreEqual(userNameTmp, userName);
        }
        [TestMethod]
        ///测试失败的情况
        public void TestMethod2()
        {
            var userName = "colin";
            var token = "adfasdfasdfasdfasdfadsfsdadsdfasdfasdf";

            var userNameTmp = string.Empty;
            var result = JsonWebToken.ValidateToken(token, out userNameTmp);
            Assert.AreNotEqual(true, result);

            Assert.AreNotEqual(userNameTmp, userName);

        }
        [TestMethod]
        ///测试失败的情况
        public void TestMethod3()
        {
            var userName = "colin";
            var token = "adfasdf.123123.dfasdfas";

            var userNameTmp = string.Empty;
            var result = JsonWebToken.ValidateToken(token, out userNameTmp);
            Assert.AreNotEqual(true, result);

            Assert.AreNotEqual(userNameTmp, userName);

        }
    }
}
