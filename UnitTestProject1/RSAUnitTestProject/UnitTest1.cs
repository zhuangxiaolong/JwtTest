using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RSAUnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var publicKey = RSAServerHelper.GetPublicKey();
            Console.WriteLine(publicKey);

            var str = "Hello World";
            var cypherText = RSAClientHelper.Encrypt(publicKey, str);
            Console.WriteLine(cypherText);

            var plainTextData = RSAServerHelper.Decrypt(cypherText);

            Assert.AreEqual(plainTextData, str);
        }
        [TestMethod]
        public void TestMethod2()
        {
            var publicKey = RSAServerHelper.GetPublicKey();
            Console.WriteLine(publicKey);

            var str = "Hello adfasdfsadf 32423423423";
            var cypherText = RSAClientHelper.Encrypt(publicKey, str);
            Console.WriteLine(cypherText);

            var plainTextData = RSAServerHelper.Decrypt(cypherText);

            Assert.AreEqual(plainTextData, str);
        }
    }
}
