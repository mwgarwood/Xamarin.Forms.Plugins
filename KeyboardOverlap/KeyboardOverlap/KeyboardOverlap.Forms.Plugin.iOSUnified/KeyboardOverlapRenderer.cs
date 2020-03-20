using System;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using KeyboardOverlap.Forms.Plugin.iOSUnified;
using System.Diagnostics;

[assembly: ExportRenderer(typeof(Page), typeof(KeyboardOverlapRenderer))]
namespace KeyboardOverlap.Forms.Plugin.iOSUnified
{
	[Preserve(AllMembers = true)]
	public class KeyboardOverlapRenderer : PageRenderer
	{
		NSObject _keyboardHideObserver;
		NSObject _keyboardChangeObserver;
		private double _pageShift = 0;
		private double _activeViewBottom;

		public static void Init()
		{
			var now = DateTime.Now;
			Debug.WriteLine("Keyboard Overlap plugin initialized {0}", now);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			var page = Element as ContentPage;

			if (page != null)
			{
				var contentScrollView = page.Content as ScrollView;

				if (contentScrollView != null)
					return;

				RegisterForKeyboardNotifications();
			}
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			UnregisterForKeyboardNotifications();
		}

		void RegisterForKeyboardNotifications()
		{
			if (_keyboardHideObserver == null)
			{
				_keyboardHideObserver = UIKeyboard.Notifications.ObserveDidHide(OnKeyboardHide);
			}
			if (_keyboardChangeObserver == null)
			{
				_keyboardChangeObserver = UIKeyboard.Notifications.ObserveDidChangeFrame(OnKeyboardChange);
			}
		}

		void UnregisterForKeyboardNotifications()
		{
			if (_keyboardHideObserver != null)
			{
				_keyboardHideObserver.Dispose();
				_keyboardHideObserver = null;
			}

			if (_keyboardChangeObserver != null)
			{
				_keyboardChangeObserver.Dispose();
				_keyboardChangeObserver = null;
			}
		}

		protected virtual void OnKeyboardChange(object sender, UIKit.UIKeyboardEventArgs args)
		{
			if (!IsViewLoaded)
				return;

			var activeView = View.FindFirstResponder();

			if (activeView == null)
				return;

			var keyboardFrame = args.FrameEnd;
			var isOverlapping = activeView.IsKeyboardOverlapping(View, keyboardFrame);

			if (!isOverlapping)
				return;

			_activeViewBottom = activeView.GetViewRelativeBottom(View);
			ShiftPageUp(keyboardFrame.Height, _activeViewBottom);
		}

		protected virtual void OnKeyboardHide(object sender, UIKit.UIKeyboardEventArgs args)
		{
			if (!IsViewLoaded)
				return;

			var keyboardFrame = args.FrameEnd;
			if (_pageShift != 0)
			{
				ShiftPageDown(keyboardFrame.Height, _activeViewBottom);
			}
		}

		private void ShiftPageUp(nfloat keyboardHeight, double activeViewBottom)
		{
			var pageFrame = Element.Bounds;
            _pageShift = CalculateShiftByAmount(pageFrame.Height, keyboardHeight, activeViewBottom);
			var newY = pageFrame.Y + _pageShift;

			Element.LayoutTo(new Rectangle(pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));
		}

		private void ShiftPageDown(nfloat keyboardHeight, double activeViewBottom)
		{
			var pageFrame = Element.Bounds;

			var newY = pageFrame.Y - _pageShift;
            _pageShift = 0;

			Element.LayoutTo(new Rectangle(pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));
		}

		private double CalculateShiftByAmount(double pageHeight, nfloat keyboardHeight, double activeViewBottom)
		{
			return (pageHeight - activeViewBottom - _pageShift) - keyboardHeight;
		}
	}
}
