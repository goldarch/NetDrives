using System;
using System.Security;
using System.Text;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Core.Shares;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_Sensitive_Data_When_Using_Encryption
  {

    [SetUp]
    public void Init()
    {
    }


    [TearDown]
    public void Cleanup()
    {
    }



    [Test]
    public void Encryption_And_Decryption_Cycle_Should_Return_Original_Value()
    {
      string plainText = @"sldkfj939ö:ösü^`^\\:..slkdDO2!";

      //encrypt plain text
      string cipher = plainText.Encrypt();
      Assert.AreNotEqual(plainText, cipher);

      //decrypt cipher into managed string
      string decrypted = cipher.Decrypt();
      Assert.AreEqual(plainText, decrypted);

      //create a SecureString from the plain text
      SecureString plainSecure = plainText.ToSecureString();

      //test unwrapping
      Assert.AreEqual(plainText, plainSecure.Unwrap());

      //encrypt the string that is wrapped into the SecureString
      string cipherFromSecure = plainSecure.Encrypt();
      
      //decrypt the cipher that was created from the the SecureString
      Assert.AreEqual(plainText, cipherFromSecure.Decrypt());

      //test empty string
      Assert.AreEqual(String.Empty, "".Encrypt().Decrypt());
    }



    [Test]
    public void Encyrption_Using_Secure_Strings_Should_Not_Alter_Value()
    {
      string plainText = @"sldkfj939ö:ösü^`^\\:..slkdDO2!";
      SecureString ss = plainText.ToSecureString();

      string cipher = ss.Encrypt();
      SecureString decrypted = cipher.DecryptSecure();

      Assert.AreEqual(ss.Length, decrypted.Length);
      Assert.AreEqual(plainText, decrypted.Unwrap());
    }


    [Test]
    public void Test_Secure_String_Comparison()
    {
      string plainText = @"sldkfj939ö:ösü^`^\\:..slkdDO2!";
      SecureString s1 = plainText.ToSecureString();
      SecureString s2 = plainText.ToSecureString();

      Assert.IsTrue(s1.Matches(s2));
      Assert.IsFalse(s1.Matches(null));
      s2 = null;
      Assert.IsFalse(s2.Matches(s1));

      s2 = "hello".ToSecureString();
      Assert.IsFalse(s1.Matches(s2));

      s1 = "".ToSecureString();
      Assert.IsFalse(s1.Matches(s2));
      s2 = "".ToSecureString();
      Assert.IsTrue(s1.Matches(s2));

      //test different strings with same length
      s1 = "s1".ToSecureString();
      s2 = "s2".ToSecureString();
      Assert.IsFalse(s1.Matches(s2));
    }


    [Test]
    public void Wrapping_And_Unwrapping_Strings_Should_Not_Alter_Content()
    {
      string s = @"sldkfj939ö:ösü^`^\\:..slkdDO2!";

      SecureString secure = s.ToSecureString();
      string unwrapped = secure.Unwrap();


      string a = Convert.ToBase64String(Encoding.Unicode.GetBytes(s));
      string b = Convert.ToBase64String(Encoding.Unicode.GetBytes(unwrapped));

      Assert.AreEqual(a, b);
      Assert.AreEqual(s, unwrapped);
    }




  }
}