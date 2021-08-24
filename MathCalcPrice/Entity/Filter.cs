namespace MathCalcPrice.Entity
{
    public class Filter
    {
        public string Shipher { get; set; }
        public bool IsEnabled { get; set; }
        public RPKShipher RPKShipher { get; set; }

        public Filter(string shipher)
        {
            Init(shipher);
        }

        public Filter(RPKShipher shipher)
        {
            Init(shipher);
        }

        public Filter Init(string shipher)
        {
            Shipher = shipher;
            RPKShipher = new RPKShipher(shipher);
            IsEnabled = true;
            return this;
        }
        public Filter Init(RPKShipher shipher)
        {
            Shipher = shipher.Code;
            RPKShipher = shipher;
            IsEnabled = true;
            return this;
        }
    }
}
