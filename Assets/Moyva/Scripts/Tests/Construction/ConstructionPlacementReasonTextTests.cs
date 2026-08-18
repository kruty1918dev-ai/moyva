#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using Kruty1918.Moyva.Construction.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Construction
{
    public sealed class ConstructionPlacementReasonTextTests
    {
        [TestCase("resources", "Недостатньо ресурсів.")]
        [TestCase("occupied-tile", "Місце вже зайняте.")]
        [TestCase("fog", "Спочатку розвідайте цю ділянку.")]
        [TestCase("authority", "Цю дію може виконати лише власник.")]
        public void Resolve_KnownCode_ReturnsPlayerFacingText(string code, string expected)
        {
            Assert.That(ConstructionPlacementReasonText.Resolve(code), Is.EqualTo(expected));
        }

        [Test]
        public void Resolve_UnknownCode_PreservesProvidedFallback()
        {
            const string fallback = "Спеціальне обмеження.";
            Assert.That(ConstructionPlacementReasonText.Resolve("custom-rule", fallback), Is.EqualTo(fallback));
        }

        [Test]
        public void Resolve_Allowed_ReturnsNoErrorText()
        {
            Assert.That(ConstructionPlacementReasonText.Resolve("allowed"), Is.Null);
        }
    }
}

#endif
