using System;

namespace Kaleidoscopic;

public class ModuleAttribute : Attribute {
    public ModuleAttribute(string description) {
        this.description = description;
    }

    public string description { get; }
}