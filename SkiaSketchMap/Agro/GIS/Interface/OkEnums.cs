//yxm created at 2019/1/4 14:07:52
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.GIS
{
	public enum OkEnvelopeEdge
	{
		okEnvelopeEdgeTopLeft,
		okEnvelopeEdgeTopMiddle,
		okEnvelopeEdgeTopRight,
		okEnvelopeEdgeMiddleLeft,
		okEnvelopeEdgeMiddleRight,
		okEnvelopeEdgeBottomLeft,
		okEnvelopeEdgeBottomMiddle,
		okEnvelopeEdgeBottomRight
	}
	public enum OkEnvelopeConstraints
	{
		okEnvelopeConstraintsNone,
		okEnvelopeConstraintsSquare,
		okEnvelopeConstraintsAspect
	}

	public enum TrackerLocation
	{
		LocationNone,
		LocationInterior,
		LocationTopLeft,
		LocationTopMiddle,
		LocationTopRight,
		LocationMiddleLeft,
		LocationMiddleRight,
		LocationBottomLeft,
		LocationBottomMiddle,
		LocationBottomRight
	}
	public enum TrackerStyle
	{
		okTrackerNormal,
		okTrackerDominant,
		okTrackerFocus,
		okTrackerActive
	}

	public enum eTextHorizontalAlignment
	{
		eTHALeft = 0,
		eTHACenter = 1,
		eTHARight = 2,
		eTHAFull = 3,
	}
	public enum eTextVerticalAlignment
	{
		eTVATop = 0,
		eTVACenter = 1,
		eTVABaseline = 2,
		eTVABottom = 3,
	}

	public enum OkScaleTextStyleEnum
	{
		okScaleTextAbsolute,
		okScaleTextRelative
	}

	public enum OkRoundingOptionEnum
	{
		OkRoundNumberOfDecimals,
		OkRoundNumberOfSignificantDigits
	}
	public enum OkNumericAlignmentEnum
	{
		okNAlignRight,
		okNAlignLeft
	}
}
