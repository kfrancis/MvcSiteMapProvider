using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcSiteMapProvider.Tests
{
    public static class Affirm
    {
        public static Affirmer That(object actual)
        {
            return new Affirmer(actual);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Affirmer
    {
        readonly object _actual;

        public Affirmer(object actual)
        {
            _actual = actual;
        }

        public void IsEqualTo(object expected)
        {
            string failureMessage = string.Format("\nExpected: <{0}>\nBut was:  <{1}>", _actual, expected);
            Assert.That(_actual.Equals(expected), Is.True, failureMessage);
        }

        public void IsNotEqualTo(object expected)
        {
            string failureMessage = string.Format("\nDid not excpect: <{0}>\nBut was:         <{1}>", _actual, expected);
            Assert.That(_actual.Equals(expected), Is.False, failureMessage);
        }
    }
}
