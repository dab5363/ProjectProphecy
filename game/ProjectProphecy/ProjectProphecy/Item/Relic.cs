using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.Entity
{
    class Relic
    {
        // --- Fields ---
        private string name;

        // --- Properties ---
        public string Name
        {
            get => name;
        }

        // --- Constructor ---
        public Relic(string name)
        {
            this.name = name;
        }
    }
}
