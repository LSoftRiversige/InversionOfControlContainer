using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests
{
    public class Warrior : IWarrior
    {
        public Warrior(string name, int power)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Power = power;
        }

        public string Name { get; set; }
        public int Power { get; set; }
    }
}
