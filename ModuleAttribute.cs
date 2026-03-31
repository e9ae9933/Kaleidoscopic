using System;

namespace Kaleidoscopic;

public class ModuleAttribute : Attribute {
    public string description { get; }

    public ModuleAttribute(string description) {
        this.description = description;
    }
}