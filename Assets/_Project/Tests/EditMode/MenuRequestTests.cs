using Guaxinim.Core;
using NUnit.Framework;

namespace Guaxinim.Tests
{
    /// <summary>O pedido de abrir o menu nos Créditos vale uma vez só.</summary>
    public class MenuRequestTests
    {
        [SetUp]
        public void SetUp() => MenuRequest.ConsumeCreditsRequest(); // limpa o que sobrou de outro teste

        [Test]
        public void ConsumeCreditsRequest_WithoutRequest_ReturnsFalse()
        {
            Assert.IsFalse(MenuRequest.ConsumeCreditsRequest());
        }

        [Test]
        public void ConsumeCreditsRequest_AfterRequest_ReturnsTrueOnlyOnce()
        {
            MenuRequest.RequestCredits();

            Assert.IsTrue(MenuRequest.ConsumeCreditsRequest());
            Assert.IsFalse(MenuRequest.ConsumeCreditsRequest());
        }
    }
}
