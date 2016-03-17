# Styling
There are two main less files you might want to change:
 
* styles.less
* epicphoto.less

The site will include the styles.css first and then the epicphoto.css afterwards.


## styles.less
This is the main Bushido template styles, it imports most of the other less files in this directory. If you change this file (or any of it's imports), there is a chance your changes it will be overwritten if we upgrade the Bushido template.

## epicphoto.less
This less file contains overrides and additional styling for the Bushido template. This is where you will want to add your changes. It imports the Bushido variables in addition to the files in the epicphoto folder. 
