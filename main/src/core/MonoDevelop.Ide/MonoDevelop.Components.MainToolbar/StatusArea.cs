// 
// StatusArea.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc. (http://xamarin.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Gtk;
using MonoDevelop.Components;
using Cairo;
using MonoDevelop.Ide;

namespace MonoDevelop.Compontents.MainToolbar
{

	public class StatusArea : EventBox
	{
		HBox contentBox = new HBox (false, 0);
		Color borderColor;

		Color fill1Color;
		Color fill2Color;

		Color innerColor;

		Color progressColor;


		bool inProgress;
		double progressFraction;

		public StatusArea ()
		{
			VisibleWindow = false;
			WidgetFlags |= Gtk.WidgetFlags.AppPaintable;
			borderColor = CairoExtensions.ParseColor ("8c8c8c");
			fill1Color = CairoExtensions.ParseColor ("eff5f7");
			fill2Color = CairoExtensions.ParseColor ("d0d9db");
			progressColor = CairoExtensions.ParseColor ("95999a");
			innerColor = CairoExtensions.ParseColor ("c4cdcf", 0.5);
			contentBox.PackStart (MonoDevelopStatusBar.messageBox, true, true, 0);
			contentBox.PackEnd (MonoDevelopStatusBar.statusIconBox, false, false, 4);
			Add (contentBox);

			this.ButtonPressEvent += delegate {
				MonoDevelopStatusBar.HandleEventMessageBoxButtonPressEvent (null, null);
			};

			MonoDevelopStatusBar.ProgressBegin += delegate {
				inProgress = true;
				progressFraction = 0;
				QueueDraw ();
			};

			MonoDevelopStatusBar.ProgressEnd += delegate {
				inProgress = false;
				progressFraction = 0;
				QueueDraw ();
			};

			MonoDevelopStatusBar.ProgressFraction += delegate(object sender, MonoDevelopStatusBar.FractionEventArgs e) {
				progressFraction = e.Work;
				QueueDraw ();
			};

			MonoDevelopStatusBar.ProgressPulse += delegate {
				// TODO
			};

			SetSizeRequest (32, 22);
			ShowAll ();
		}

		protected override void OnSizeRequested (ref Requisition requisition)
		{
			requisition.Height = 22;
			base.OnSizeRequested (ref requisition);
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (var context = Gdk.CairoHelper.Create (evnt.Window)) {
				CairoExtensions.RoundedRectangle (context, Allocation.X + 0.5, Allocation.Y + 0.5, Allocation.Width, Allocation.Height, 4);
				LinearGradient lg = new LinearGradient (Allocation.X, Allocation.Y, Allocation.X, Allocation.Height);
				lg.AddColorStop (0, fill1Color);
				lg.AddColorStop (1, fill2Color);
				context.Pattern = lg;
				context.FillPreserve ();

				context.LineWidth = 4;
				context.Color = innerColor;
				context.StrokePreserve ();

				if (inProgress) {
					context.NewPath ();
					context.Rectangle (Allocation.X + 0.5, Allocation.Y + 0.5, Allocation.Width * progressFraction, Allocation.Height);
					context.Clip ();
					CairoExtensions.RoundedRectangle (context, Allocation.X + 0.5, Allocation.Y + 0.5, Allocation.Width, Allocation.Height, 4);
					context.Color = progressColor;
					context.FillPreserve ();
					context.ResetClip ();
				}

				context.LineWidth = 1;
				context.Color = borderColor;
				context.Stroke ();
			}
			return base.OnExposeEvent (evnt);
		}

	}
}

