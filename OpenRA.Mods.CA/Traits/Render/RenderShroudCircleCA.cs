#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.CA.Traits
{
	public enum RangeCircleVisibility { Always, WhenSelected }

	class RenderShroudCircleCAInfo : ITraitInfo
	{
		[Desc("Color of the circle.")]
		public readonly Color Color = Color.FromArgb(128, Color.Cyan);

		[Desc("Contrast color of the circle.")]
		public readonly Color ContrastColor = Color.FromArgb(96, Color.Black);

		[Desc("When to show the range circle. Valid values are `Always`, and `WhenSelected`")]
		public readonly RangeCircleVisibility Visible = RangeCircleVisibility.WhenSelected;

		public object Create(ActorInitializer init) { return new RenderShroudCircleCA(init.Self, this); }
	}

	class RenderShroudCircleCA : INotifyCreated, IRenderAnnotationsWhenSelected, IRenderAnnotations
	{
		readonly RenderShroudCircleCAInfo info;
		WDist range;

		public RenderShroudCircleCA(Actor self, RenderShroudCircleCAInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			range = self.TraitsImplementing<CreatesShroud>()
				.Select(cs => cs.Info.Range)
				.DefaultIfEmpty(WDist.Zero)
				.Max();
		}

		public IEnumerable<IRenderable> RangeCircleRenderables(Actor self, WorldRenderer wr, RangeCircleVisibility visibility)
		{
			if (!self.Owner.IsAlliedWith(self.World.RenderPlayer))
				yield break;

			if (info.Visible == visibility)
				yield return new RangeCircleAnnotationRenderable(
					self.CenterPosition,
					range,
					0,
					info.Color,
					info.ContrastColor);
		}

		IEnumerable<IRenderable> IRenderAnnotationsWhenSelected.RenderAnnotations(Actor self, WorldRenderer wr)
		{
			return RangeCircleRenderables(self, wr, RangeCircleVisibility.WhenSelected);
		}

		bool IRenderAnnotationsWhenSelected.SpatiallyPartitionable { get { return false; } }

		IEnumerable<IRenderable> IRenderAnnotations.RenderAnnotations(Actor self, WorldRenderer wr)
		{
			return RangeCircleRenderables(self, wr, RangeCircleVisibility.Always);
		}

		bool IRenderAnnotations.SpatiallyPartitionable { get { return false; } }
	}
}
