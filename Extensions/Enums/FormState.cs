using System.Drawing;

namespace Extensions;

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
	public static bool IsNormal(this FormState state)
	{
		return state is FormState.NormalUnfocused or FormState.NormalFocused;
	}

	public static FormState Normal(bool active)
	{
		return active ? FormState.NormalFocused : FormState.NormalUnfocused;
	}

	public static Color Color(this FormState state)
	{
		return state switch
		{
			FormState.NormalFocused or FormState.ForcedFocused => FormDesign.Design.BackColor.MergeColor(FormDesign.Design.ActiveColor, 55),
			FormState.Busy => FormDesign.Design.RedColor,
			FormState.Working => FormDesign.Design.YellowColor,
			FormState.Running => FormDesign.Design.GreenColor,
			FormState.Active => FormDesign.Design.ActiveColor,
			_ => FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type == FormDesignType.Dark ? 3 : -3),
		};
	}
}