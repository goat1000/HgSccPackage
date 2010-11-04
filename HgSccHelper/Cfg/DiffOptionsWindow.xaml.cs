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
using System.Windows;
using System.IO;
using Path = System.IO.Path;

namespace HgSccHelper
{
	/// <summary>
	/// Interaction logic for DiffOptionsWindow.xaml
	/// </summary>
	public partial class DiffOptionsWindow : Window
	{
		private List<MergeToolInfo> merge_tools;

		//------------------------------------------------------------------
		public DiffOptionsWindow()
		{
			InitializeComponent();

			comboTool.Items.Clear();

			merge_tools = new HgMergeTools().GetMergeTools();
			foreach (var tool in merge_tools)
			{
				comboTool.Items.Add(new DiffComboItem
				{
					DiffTool = tool.ExecutableFilename,
					DiffArgs = tool.DiffArgs
				});
			}

			if (HgSccOptions.Options.DiffTool.Length != 0)
				AddDiffTool(HgSccOptions.Options.DiffTool, HgSccOptions.Options.DiffArgs);
		}

		//------------------------------------------------------------------
		private void Browse_Click(object sender, RoutedEventArgs e)
		{
			string diff_tool = comboTool.Text;
			if (HgOptionsHelper.BrowseDiffTool(ref diff_tool))
			{
				AddDiffTool(diff_tool, "");
			}
		}

		//-----------------------------------------------------------------------------
		private void AddDiffTool(string diff_tool, string diff_args)
		{
			var new_tool = merge_tools.Find(tool =>
											  String.Compare(tool.ExecutableFilename, diff_tool, true) == 0);

			if (new_tool == null)
			{
				new_tool = new MergeToolInfo(Path.GetFileNameWithoutExtension(diff_tool));
				new_tool.Executable = diff_tool;
				if (!string.IsNullOrEmpty(diff_args))
					new_tool.DiffArgs = diff_args;

				new_tool.FindExecutable();
			}

			foreach (DiffComboItem item in comboTool.Items)
			{
				if (String.Compare(item.DiffTool, new_tool.ExecutableFilename, true) == 0)
				{
					if (!string.IsNullOrEmpty(diff_args))
						item.DiffArgs = diff_args;
					else
						item.DiffArgs = new_tool.DiffArgs;

					comboTool.SelectedItem = item;
					textArgs.Text = item.DiffArgs;
					return;
				}
			}

			comboTool.Items.Add(new DiffComboItem { DiffTool = new_tool.ExecutableFilename, DiffArgs = new_tool.DiffArgs });
			comboTool.SelectedIndex = comboTool.Items.Count - 1;
		}

		//------------------------------------------------------------------
		public string DiffToolPath
		{
			get { return comboTool.Text; }
		}

		//------------------------------------------------------------------
		public string DiffToolArgs
		{
			get { return textArgs.Text; }
		}

		//------------------------------------------------------------------
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			var diff_tool = DiffToolPath;

			if (diff_tool.Length != 0)
			{
				if (!File.Exists(diff_tool))
				{
					MessageBox.Show("File: " + diff_tool + " is not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			HgSccOptions.Options.DiffTool = diff_tool;
			HgSccOptions.Options.DiffArgs = DiffToolArgs;
			HgSccOptions.Save();

			DialogResult = true;
		}

		//-----------------------------------------------------------------------------
		private void comboTool_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var item = comboTool.SelectedItem as DiffComboItem;
			if (item != null)
				textArgs.Text = item.DiffArgs;
		}
	}
}
