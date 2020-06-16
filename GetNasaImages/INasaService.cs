using System;
using System.Collections.Generic;
using System.Text;

namespace GetNasaImages
{
	public interface INasaService
	{
		NasaAPOD getAPOD(bool isHD = false, DateTime? date = null);

		byte[] getAstronomicPicOfDay(bool isHD = false, DateTime? date = null);		
	}
}
