using Bot;
using static NUnit.Framework.Assert;

namespace Tests;

public class KeyboardGeneratorTests
{
    [Test]
    public void GenerateKeyboard3_2_1()
    {
        var items = new List<string>
        {
            "analisi 1",
            "analisi 2",
            "analisi 3",
            "Lorem ipsum dolor sit amet, co",
            "Lorem ipsum dolor sit amet, co",
            "Lorem ipsum dolor sit amet, co"
        };
        var markup = KeyboardGenerator.GenerateKeyboardMarkup(items,false);

        That(markup.Keyboard.ElementAt(0).Count(), Is.EqualTo(3));
        That(markup.Keyboard.ElementAt(1).Count(), Is.EqualTo(2));
        That(markup.Keyboard.ElementAt(2).Count(), Is.EqualTo(1));
    }
    
    [Test]
    public void GenerateKeyboard3_3_3()
    {
        var items = new List<string>
        {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            
        };
        var markup = KeyboardGenerator.GenerateKeyboardMarkup(items,false);

        That(markup.Keyboard.ElementAt(0).Count(), Is.EqualTo(3));
        That(markup.Keyboard.ElementAt(1).Count(), Is.EqualTo(3));
        That(markup.Keyboard.ElementAt(2).Count(), Is.EqualTo(3));
    }
    
    [Test]
    public void GenerateKeyboard2_2_3()
    {
        var items = new List<string>
        {
            "Lorem ipsum dolor sit amet, consectetuer",
            "Lorem ipsum dolor sit amet, consectetuer",
            "analisi 3",
            "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commo",
            "analisi 4",
            "4",
            "5"
        };
        var markup = KeyboardGenerator.GenerateKeyboardMarkup(items,false);

        That(markup.Keyboard.ElementAt(0).Count(), Is.EqualTo(2));
        That(markup.Keyboard.ElementAt(1).Count(), Is.EqualTo(2));
        That(markup.Keyboard.ElementAt(2).Count(), Is.EqualTo(3));
    }
}