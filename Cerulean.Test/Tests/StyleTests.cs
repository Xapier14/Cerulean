namespace Cerulean.Test
{
    [TestFixture]
    public static class StyleTests
    {
        [Test]
        public static void ApplyPositionStyleToLabel_MustBe_200_300()
        {
            var positionStyle = new Style();
            positionStyle.AddSetter("X", 200);
            positionStyle.AddSetter("Y", 300);

            var label = new Label();
            positionStyle.ApplyStyle(label);
            Assert.Multiple(() =>
            {
                Assert.That(label.X, Is.EqualTo(200), "Label.X is not 200.");
                Assert.That(label.Y, Is.EqualTo(300), "Label.Y is not 300.");
            });
        }
    }
}
