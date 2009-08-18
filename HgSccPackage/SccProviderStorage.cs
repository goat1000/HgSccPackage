//=========================================================================
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using HgSccHelper;
using C5;

namespace HgSccPackage
{
	// This class defines basic source control status values
	public enum SourceControlStatus
	{
		scsUncontrolled = 0,
		scsCheckedIn,
		scsCheckedOut
	};

	//------------------------------------------------------------------
	public class SourceControlInfo
	{
		public string File { get; set; }
		public SourceControlStatus Status { get; set; }
	};

	//==================================================================
	public class SccProviderStorage
	{
		private HgScc hgscc;
		private HashDictionary<string, HgFileInfo> cache;

		//------------------------------------------------------------------
		public SccProviderStorage()
		{
			cache = new HashDictionary<string, HgFileInfo>();
		}

		//------------------------------------------------------------------
		public bool IsValid
		{
			get
			{
				return hgscc != null && !String.IsNullOrEmpty(hgscc.WorkingDir); 
			}
		}

		//------------------------------------------------------------------
		public SccErrors Init(string work_dir, SccOpenProjectFlags flags)
		{
			Logger.WriteLine("SccProviderStorage: {0}", work_dir);
			if (hgscc == null)
				hgscc = new HgScc();

			var err = hgscc.OpenProject(work_dir, flags);
			if (err == SccErrors.Ok)
				ReloadCache();

			return err;
		}

		//------------------------------------------------------------------
		public void ReloadCache()
		{
			if (!IsValid)
				return;
			
			ResetCache();
			
			foreach (var pair in hgscc.QueryInfoFullDict())
			{
				var file = pair.Key;
				var status = pair.Value;

				cache[Path.Combine(hgscc.WorkingDir, file).ToLower()] = status;
			}
		}

		//------------------------------------------------------------------
		public void Close()
		{
			hgscc = null;
			cache.Clear();
		}

		//------------------------------------------------------------------
		/// <summary>
		/// Adds files to source control by adding them to the list of "controlled" files in the current project
		/// and changing their attributes to reflect the "checked in" status.
		/// </summary>
		public SccErrors AddFilesToStorage(IEnumerable<string> files)
		{
			if (!IsValid)
				return SccErrors.UnknownError;

			var files_list = new List<string>(files);
			var status_list = new SourceControlStatus[files_list.Count];

			var status_error = GetStatusForFiles(files_list.ToArray(), status_list);

			var lst = new List<SccAddFile>();
			for (int i = 0; i < files_list.Count; ++i)
			{
				if (status_list[i] == SourceControlStatus.scsUncontrolled)
				{
					var f = files_list[i];
					lst.Add(new SccAddFile { File = f, Flags = SccAddFlags.FileTypeAuto });
					Logger.WriteLine("Adding: {0}", f);
				}
			}

			if (lst.Count == 0)
				return SccErrors.Ok;

			var err = hgscc.Add(IntPtr.Zero, lst.ToArray(), "Adding files");
			if (err == SccErrors.Ok)
			{
/*
				// �.�. ��� ���������� ������ ����� ���������� ������,
				// �� ���������� ���
				ResetCache();
*/
				UpdateCache(files);
			}

			return err;
		}

		//------------------------------------------------------------------
		/// <summary>
		/// Renames a "controlled" file. If the project file is being renamed, rename the whole storage file
		/// </summary>
		public SccErrors RenameFileInStorage(string strOldName, string strNewName)
		{
			// FIXME: RenameFiles!!
			if (!IsValid)
				return SccErrors.UnknownError;

			Logger.WriteLine("Rename: {0} to {1}", strOldName, strNewName);

			var err = hgscc.Rename(IntPtr.Zero, strOldName, strNewName);
			if (err == SccErrors.Ok)
			{
/*
				// �.�. ��� �������������� ������ ����� ���������� ������,
				// �� ���������� ���
				ResetCache();
*/
				UpdateCache(new[]{strOldName, strNewName});
			}

			return err;
		}

		//------------------------------------------------------------------
		private static SourceControlStatus FromHgStatus(HgFileStatus status)
		{
			switch (status)
			{
				case HgFileStatus.Added:
					return SourceControlStatus.scsCheckedOut;
				case HgFileStatus.Clean:
					return SourceControlStatus.scsCheckedIn;
				case HgFileStatus.Deleted:
					return SourceControlStatus.scsUncontrolled;
				case HgFileStatus.Ignored:
					return SourceControlStatus.scsUncontrolled;
				case HgFileStatus.Modified:
					return SourceControlStatus.scsCheckedOut;
				case HgFileStatus.NotTracked:
					return SourceControlStatus.scsUncontrolled;
				case HgFileStatus.Removed:
					return SourceControlStatus.scsUncontrolled;
				case HgFileStatus.Tracked:
					return SourceControlStatus.scsCheckedIn;
			}

			return SourceControlStatus.scsUncontrolled;
		}

		//------------------------------------------------------------------
		private static HgFileStatus ToHgStatus(SourceControlStatus status)
		{
			switch (status)
			{
				case SourceControlStatus.scsCheckedIn:
					return HgFileStatus.Tracked;
				case SourceControlStatus.scsCheckedOut:
					return HgFileStatus.Modified;
				case SourceControlStatus.scsUncontrolled:
					return HgFileStatus.NotTracked;
			}

			return HgFileStatus.NotTracked;
		}

		//------------------------------------------------------------------
		public SccErrors GetStatusForFiles(SourceControlInfo[] files)
		{
			if (!IsValid)
				return SccErrors.UnknownError;

			var not_in_cache = new List<string>();

			foreach (var file in files)
			{
				if (!cache.Contains(file.File.ToLower()))
					not_in_cache.Add(file.File);
			}

			if (not_in_cache.Count != 0)
			{
				UpdateCache(not_in_cache);
			}

			foreach (var file in files)
			{
				HgFileInfo info;
				if (cache.Find(file.File.ToLower(), out info))
				{
					file.Status = FromHgStatus(info.Status);
				}
				Logger.WriteLine("GetFileStatus: {0} = {1}", file.File, file.Status);
			}

			return SccErrors.Ok;
		}

		//------------------------------------------------------------------
		public SccErrors GetStatusForFiles(string[] files, SourceControlStatus[] statuses)
		{
			if (!IsValid)
				return SccErrors.UnknownError;

			var not_in_cache = new List<string>();

			foreach (var file in files)
			{
				if (!cache.Contains(file.ToLower()))
					not_in_cache.Add(file);
			}

			if (not_in_cache.Count != 0)
			{
				UpdateCache(not_in_cache);
			}

			for (int i = 0; i < files.Length; ++i)
			{
				HgFileInfo info;
				if (cache.Find(files[i].ToLower(), out info))
				{
					statuses[i] = FromHgStatus(info.Status);
				}

				Logger.WriteLine("GetFileStatus: {0} = {1}", files[i], statuses[i]);
			}

			return SccErrors.Ok;
		}

		//------------------------------------------------------------------
		public SccErrors Commit(IEnumerable<string> files, out IEnumerable<string> commited_files)
		{
			if (!IsValid)
			{
				commited_files = new List<string>();
				return SccErrors.UnknownError;
			}

			foreach (var f in files)
			{
				Logger.WriteLine("Commit: {0}", f);
			}


			var error = hgscc.Commit(IntPtr.Zero, files, "", out commited_files);
			if (error == SccErrors.Ok)
			{
				UpdateCache(commited_files);
			}
			return error;
		}

		//------------------------------------------------------------------
		public SccErrors Revert(IEnumerable<string> files, out IEnumerable<string> reverted_files)
		{
			if (!IsValid)
			{
				reverted_files = new List<string>();
				return SccErrors.UnknownError;
			}

			foreach (var f in files)
			{
				Logger.WriteLine("Revert: {0}", f);
			}


			var error = hgscc.Revert(IntPtr.Zero, files, out reverted_files);
			if (error == SccErrors.Ok)
			{
//				UpdateCache(reverted_files);
				ReloadCache();
			}
			return error;
		}

		//------------------------------------------------------------------
		public SccErrors RemoveFiles(IEnumerable<string> files)
		{
			if (!IsValid)
				return SccErrors.UnknownError;

			var error = hgscc.Remove(IntPtr.Zero, files, "");
			if (error != SccErrors.Ok)
				return error;

			foreach (var f in files)
			{
				Logger.WriteLine("Remove: {0}", f);
				SetCacheStatus(f, SourceControlStatus.scsCheckedOut);
			}
			return SccErrors.Ok;
		}

		//------------------------------------------------------------------
		/// <summary>
		/// Returns a source control status inferred from the file's attributes on local disk
		/// </summary>
		public SourceControlStatus GetFileStatus(string filename)
		{
			var info = new SourceControlInfo { File = filename };
			var lst = new SourceControlInfo[]{ info };

			GetStatusForFiles(lst);

			return lst[0].Status;
		}

		//------------------------------------------------------------------
		public void ViewHistory(string filename)
		{
			if (!IsValid)
				return;

			hgscc.History(IntPtr.Zero, filename);
		}

		//------------------------------------------------------------------
		public void RemoveFile(string filename)
		{
			var files = new[] {filename};
			RemoveFiles(files);
		}

		//------------------------------------------------------------------
		private void UpdateCache(IEnumerable<string> files)
		{
			if (!IsValid)
				return;

			var lst = new List<HgFileInfo>();
			foreach (var f in files)
			{
				var info = new HgFileInfo {File = f, Status = HgFileStatus.NotTracked};
				lst.Add(info);
				
				Logger.WriteLine("UpdateCache: {0}", f);
			}
			
			var info_lst = lst.ToArray();

			SccErrors error = hgscc.QueryInfo2(info_lst);
			if (error == SccErrors.Ok)
			{
				foreach (var info in info_lst)
				{
					cache[info.File.ToLower()] = info;
				}
			}
		}

		//------------------------------------------------------------------
		private void ResetCache()
		{
			cache.Clear();
		}

		//------------------------------------------------------------------
		public void UpdateFileCache(string file)
		{
			UpdateCache(new[] {file});
		}

		//------------------------------------------------------------------
		public void SetCacheStatus(string file, SourceControlStatus status)
		{
			Logger.WriteLine("SetCacheStatus: {0}, {1}", file, status);
			
			HgFileInfo info;
			if (cache.Find(file.ToLower(), out info))
			{
				info.Status = ToHgStatus(status);
			}
			else
			{
				Logger.WriteLine("File not found in cache");
			}
		}

		//------------------------------------------------------------------
		public void Compare(string file)
		{
			if (!IsValid)
				return;

			hgscc.Diff(IntPtr.Zero, file, SccDiffFlags.None);
		}

		//------------------------------------------------------------------
		public void ViewChangeLog()
		{
			if (!IsValid)
				return;

			RevLogWindow wnd = new RevLogWindow();
			wnd.WorkingDir = hgscc.WorkingDir;
			wnd.ShowDialog();
		}

		//------------------------------------------------------------------
		public void Synchronize()
		{
			if (!IsValid)
				return;

			var wnd = new SynchronizeWindow();
			wnd.WorkingDir = hgscc.WorkingDir;
			wnd.ShowDialog();
		}
	}
}