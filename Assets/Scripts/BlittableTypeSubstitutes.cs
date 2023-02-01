// TODO: JPB: (refactor) Remove Bool and Char structs once we have blittable bools/chars or use IComponentData
public readonly struct Bool {
    private readonly byte _val;
    public Bool(bool b) {
        if (b) {
            _val = 1;
        } else {
            _val = 0;
        }
    }
    public static implicit operator bool(Bool b) => b._val != 0;
    public static implicit operator Bool(bool b) => new Bool(b);
}

public readonly struct Char {
    private readonly byte _val;
    public Char(char c) {
        _val = (byte)c;
    }
    public static implicit operator char(Char c) => (char)c._val;
    public static implicit operator Char(char c) => new Char(c);
}
