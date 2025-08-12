using MusicFinder;

namespace MusicFinderTests
{
	[TestClass]
	public sealed class SloppyAnswerComparerTests
	{
		[TestMethod]
		public void AreCloseEnough_ExactMatch_ReturnsTrue()
		{
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("Queen", "Queen"));
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("The The", "The The"));   // Special case with double "The" (first instance is ignored)
		}

		[TestMethod]
		public void AreCloseEnough_CaseInsensitive_ReturnsTrue()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("Bohemian Rhapsody", "bohemian rhapsody");
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void AreCloseEnough_LeadingThe_ReturnsTrue()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("The Beatles", "Beetles");
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void AreCloseEnough_MinorTypo_ReturnsTrue()
		{
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("Nirvana", "Nirvanna"));
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("X", "Y"));
		}

		[TestMethod]
		public void AreCloseEnough_PunctuationAndWhitespace_ReturnsTrue()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("AC/DC!", "ac dc");
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void AreCloseEnough_NullOrEmpty_ReturnsFalse()
		{
			Assert.IsFalse(SloppyAnswerComparer.AreCloseEnough(null, "test guess"));
			Assert.IsFalse(SloppyAnswerComparer.AreCloseEnough("test artist", null));
			Assert.IsFalse(SloppyAnswerComparer.AreCloseEnough("", "test guess"));
			Assert.IsFalse(SloppyAnswerComparer.AreCloseEnough("test artist", ""));
			Assert.IsFalse(SloppyAnswerComparer.AreCloseEnough("test artist", "?"));    // Only punctuation (boils down to an empty string)
			Assert.IsFalse(SloppyAnswerComparer.AreCloseEnough("X", "??????"));
		}

		[TestMethod]
		public void AreCloseEnough_DifferentStrings_ReturnsFalse()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("Radiohead", "Coldplay");
			Assert.IsFalse(result);
		}
	}
}
