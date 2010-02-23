﻿//=========================================================================
// Copyright 2009 Sergey Antonov <sergant_@mail.ru>
// 
// This software may be used and distributed according to the terms of the
// GNU General Public License version 2 as published by the Free Software
// Foundation.
// 
// See the file COPYING.TXT for the full text of the license, or see
// http://www.gnu.org/licenses/gpl-2.0.txt
// 
//=========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HgSccHelper
{
	/// <summary>
	/// Interaction logic for PathEditWindow.xaml
	/// </summary>
	public partial class PathEditWindow : Window
	{
		//-----------------------------------------------------------------------------
		public string Alias
		{
			get { return (string)GetValue(AliasProperty); }
			set { SetValue(AliasProperty, value); }
		}

		//-----------------------------------------------------------------------------
		public static readonly DependencyProperty AliasProperty =
			DependencyProperty.Register("Alias", typeof(string), typeof(PathEditWindow));

		//-----------------------------------------------------------------------------
		public string Path { get; set; }

		//------------------------------------------------------------------
		public string WorkingDir { get; set; }

		//-----------------------------------------------------------------------------
		private string Url
		{
			get { return (string)GetValue(UrlProperty); }
			set { SetValue(UrlProperty, value); }
		}

		//-----------------------------------------------------------------------------
		public static readonly DependencyProperty UrlProperty =
			DependencyProperty.Register("Url", typeof(string), typeof(PathEditWindow),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(UrlChanged)));

		//-----------------------------------------------------------------------------
		static void UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			var value = (string)args.NewValue;

			try
			{
				var uri = new Uri(value);
				var builder = new UriBuilder(value);
				var path_edit_wnd = d as PathEditWindow;

				if (!uri.IsFile)
				{
					if (!string.IsNullOrEmpty(builder.Password))
					{
						if (path_edit_wnd != null)
							path_edit_wnd.OnUrlPasswordChange(builder.Password);

						builder.Password = "";
						d.SetValue(args.Property, builder.Uri.AbsoluteUri);
					}

					if (!String.IsNullOrEmpty(builder.UserName))
						path_edit_wnd.OnUrlUserChange(builder.UserName);
				}
			}
			catch (UriFormatException)
			{
			}
		}

		//-----------------------------------------------------------------------------
		private string Username
		{
			get { return (string)GetValue(UsernameProperty); }
			set { SetValue(UsernameProperty, value); }
		}

		//-----------------------------------------------------------------------------
		public static readonly DependencyProperty UsernameProperty =
			DependencyProperty.Register("Username", typeof(string), typeof(PathEditWindow));

		//-----------------------------------------------------------------------------
		public void OnUrlPasswordChange(string password)
		{
			passwordBox.Password = password;
		}

		//-----------------------------------------------------------------------------
		public void OnUrlUserChange(string user)
		{
			Username = user;
		}

		//-----------------------------------------------------------------------------
		public PathEditWindow()
		{
			InitializeComponent();
		}

		//-----------------------------------------------------------------------------
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Url = Path;

			textAlias.SelectAll();
			textUrl.SelectAll();
			textUsername.SelectAll();
			passwordBox.SelectAll();

			textAlias.Focus();

			UpdateEnabledStatus();
		}

		//-----------------------------------------------------------------------------
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Path = Url;

			if (string.IsNullOrEmpty(Alias))
			{
				MessageBox.Show("You must enter non empty alias", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			if (string.IsNullOrEmpty(Path))
			{
				MessageBox.Show("You must enter non empty url", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			try
			{
				var uri = new Uri(Url);
				if (!uri.IsFile)
				{
					if (	!String.IsNullOrEmpty(uri.Host)
						&&	!String.IsNullOrEmpty(uri.Scheme)
						)
					{
						var builder = new UriBuilder(Url);

						if (!String.IsNullOrEmpty(Username))
							builder.UserName = Username;
						if (!String.IsNullOrEmpty(passwordBox.Password))
							builder.Password = passwordBox.Password;

						Path = builder.Uri.AbsoluteUri;
					}
				}
			}
			catch (UriFormatException)
			{
			}

			DialogResult = true;
			Close();
		}

		//-----------------------------------------------------------------------------
		private void textUsername_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Username == textUsername.Text)
			{
				// Changed by binding property, no need to update Url
				return;
			}

			try
			{
				if (Util.IsValidRemoteUrl(Url))
				{
					var builder = new UriBuilder(Url);
					builder.UserName = textUsername.Text;

					// Make sure, that we entering username with allowed characters
					var checker = new UriBuilder(builder.Uri.AbsoluteUri);
					if (checker.UserName == textUsername.Text)
						Url = builder.Uri.AbsoluteUri;
				}
			}
			catch (UriFormatException)
			{
			}
		}

		//-----------------------------------------------------------------------------
		void UpdateEnabledStatus()
		{
			bool is_valid_remote_url = Util.IsValidRemoteUrl(textUrl.Text);

			textUsername.IsEnabled = is_valid_remote_url;
			passwordBox.IsEnabled = is_valid_remote_url;
		}

		//-----------------------------------------------------------------------------
		private void textUrl_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateEnabledStatus();
			Url = textUrl.Text;
		}

		//-----------------------------------------------------------------------------
		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
			{
				dlg.Description = "Browse for Repository...";
				dlg.ShowNewFolderButton = false;

				if (!String.IsNullOrEmpty(Url)
					&& System.IO.Directory.Exists(Url)
					)
				{
					dlg.SelectedPath = textUrl.Text;
				}
				else
				{
					dlg.SelectedPath = WorkingDir;
				}

				var result = dlg.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					textUrl.Text = dlg.SelectedPath;
				}
			}
		}
	}
}