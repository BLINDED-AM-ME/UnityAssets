using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLINDED_AM_ME.Objects
{
	// This is why we want properties instead of feilds in the inspector
	public class DataModel : INotifyPropertyChanged
	{
		private string _id;
		public string Id
		{
			get => _id;
			set => SetProperty(ref _id, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <returns> true if property was changed </returns>
		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (Equals(storage, value))
				return false;

			storage = value;
			OnPropertyChanged(propertyName);
			return true;
		}
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	
	}
}
