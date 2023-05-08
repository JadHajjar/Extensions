using System.Drawing;

namespace Extensions
{
	public enum FormState
	{
		NormalUnfocused = 0,
		NormalFocused = 1,
		ForcedFocused = 2,
		Active = 3,
		Busy = 4,
		Working = 5,
		Running = 6,
	}

	public static class FormStateExt
	{
		public static bool IsNormal(this FormState state) => state == FormState.NormalUnfocused || state == FormState.NormalFocused;

		public static FormState Normal(bool active) => active ? FormState.NormalFocused : FormState.NormalUnfocused;

		public static Color Color(this FormState state)
		{
			switch (state)
			{
				case FormState.NormalFocused:
				case FormState.ForcedFocused:
					return FormDesign.Design.BackColor.MergeColor(FormDesign.Design.ActiveColor, 55);

				case FormState.Busy:
					return FormDesign.Design.RedColor;

				case FormState.Working:
					return FormDesign.Design.YellowColor;

				case FormState.Running:
					return FormDesign.Design.GreenColor;

				case FormState.Active:
					return FormDesign.Design.ActiveColor;

				default:
					return FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type == FormDesignType.Dark ? 3 : -3);
			}
		}
	}
}