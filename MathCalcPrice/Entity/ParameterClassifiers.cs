namespace MathCalcPrice.Entity
{
    public class ParameterClassifiers
    {
        public string Id { get; set; }
        public string ClassParams { get; set; }
        public string ClassConstruction { get; set; }
        public string ClassMaterial { get; set; }
        public string ClassSection { get; set; }
        public string ClassFloor { get; set; }
        public string ClassChars { get; set; }
        public string GroupName { get; set; }

        public bool IsValid()
        {

            if (this.ClassParams == null || this.ClassParams == "")
                return false;

            if (this.ClassConstruction == null || this.ClassConstruction == "")
                return false;

            if (this.ClassMaterial == null || this.ClassMaterial == "")
                return false;

            if (this.ClassSection == null || this.ClassSection == "")
                return false;

            if (this.ClassFloor == null || this.ClassFloor == "")
                return false;

            if (this.ClassChars == null || this.ClassChars == "")
                return false;

            return true;
        }
    }
}
