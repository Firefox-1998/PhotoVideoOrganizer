# PhotoMoveYearMonthFolder

How many of you have thousands of photos scattered everywhere
(cloud, folders, external hard drives, USB sticks, etc. etc.)? 

I created this software, completely free, to organize my photos/videos. 

The software searches photos, videos and more, organizing the files into
subfolders, by year and month and, importantly, discarding duplicates.

Example:
	search in folder 1 and folder 2
	results
	folder 3
		1970  
			01
				picture1.jpg
				picture2.jpg
		2022
			01
				picture77.jpg
			02
				picture10.jpg
				picture11.jpg
			08
				picture23.jpg
				picture24.jpg
		2023
			04
				...
			05
			06
			07
			12
		2024
			01
			02
			03
			04
			05
			
	(folder 1970/01 is used when the software cannot find a valid date)			
			
Different methods are used for find a duplicates, the hash of the file 
and UniqueImageID from the Exif data. 
Also, to find the shooting date, the file name is used (often the name 
itself contains date and time) or Exif data: DateTimeDigitized, DateTimeOriginal or DateTime.

This software is released under the MIT license

[2024] [Giovanni Limongiello aka Firefox_1998]