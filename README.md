# Epic Photo
An Episerver Commerce project you can use for demos, learning from and possibly base your next web site on.

![Start Page](https://raw.githubusercontent.com/BVNetwork/CommerceStarterKit/master/doc/img/screenshots/readme-start-page.png)

## Getting Started
1. Clone the repository
2. Download the databases from [Dropbox](https://dl.dropboxusercontent.com/u/3403147/2016-02-07-epicphoto.zip) (temporary location for now) 
3. Unzip the databases to the /db/ folder
4. Open the solution in Visual Studio 2013 or 2015
5. Run it (also see [How to Start Commerce Manager](#how-to-start-commerce-manager))
6. Default admin account for web and manager sites is: **admin \ st0re**
7. Configure dependencies for more features (see below)

**Note!** In case you update the source and want to keep your existing database, please read the section on [how to upgrade the CMS and Commerce databases](#how-to-upgrade-the-databases).

## Configuring Dependencies
The Commerce Starter Kit has these dependencies:

### Episerver Find
Episerver Find is used in most product lists, as the wine and fashion lists, and also by the configurable search block. The main search and the search-as-you-type function also uses Find.

You will have a hard time using the site without Find.

**Configuration**

1. Go to [http://find.episerver.com](http://find.episerver.com), log in, and create a new developer index (with English and Norwegian languages).
2. Add the `<episerver.find ... >` configuration to web.config. Search for it, it is already in the web.config, but with invalid configuration settings. 
2. Go to [http://localhost:49883/episerver/CMS/Admin/Default.aspx](http://localhost:49883/episerver/CMS/Admin/Default.aspx "Admin mode"), find the "Index Product Catalog" Scheduled Job and run it.
3. Verify that the product list is showing correctly by visiting one of the product categories.

### DIBS
The solution can use DIBS as payment method (among others). If you want to use this payment method and your company does not already have a demo account with DIBS, you need to sign up for one: [http://www.dibspayment.com/demo-signup](http://www.dibspayment.com/demo-signup) The starter kit is using the [DIBS D2 platform](http://tech.dibspayment.com/dibs_payment_window).

You need the following settings from your DIBS account:

* API UserId (this is your merchant ID)
* HMAC key for MAC calculation (K)

Configure payment settings:

1. Log in to Commerce Manager
2. Open the Administration menu
3. Expand Order System / Payments / English (repeat for "norsk")
4. Select Parameters tab
5. Configure the settings according to the values above

**NOTE!** You need to configure the DIBS account for both English and "norsk" in Commerce Manager. 

This can be found on the [DIBS administration web site](https://payment.architrade.com/login/login.action)

You can find a [list of test cards](http://tech.dibspayment.com/toolbox/test_information_cards) on the DIBS web site. Use these to test the checkout process.

### Google Analytics
The starter kit has extended ecommerce tracking, which will start tracking when you configure the Episerver Google Analytics Add-on.

Make sure your Google Analytics account is using Universal Analytics and has the Enhanced Ecommerce feature enabled.

By default, the following is tracked:

* Page views
* Add to cart
* View cart
* Checkout
* Payment
* Configurable Search Blocks 

# Nice to Know
Various helpful stuff

## How to Start Commerce Manager
When you start the project in Visual Studio, it will only start the the default site (the designated Startup Project). This is typically the web site and not Commerce Manager.

To start Commerce Manager, right click the CommerceStarterKit.Commerce project in Visual Studio, View, View in Browser (or Ctrl+Shift+W)

## Hot to start the B2B Site
There is a B2B site included, which is set up to run on localhost:49900 in IIS Express by default. Your local IIS Express (the one that runs when you start the project in Visual Studio) will not know how to resolve this addess. You can add this binding yourself by editing the applicationhost.config file for IIS Express.

If you use Visual Studio 2015, open `SoltutionDir\.vs\config\applicationhost.config`

If you use Visual Studio 2013, open `%USERPROFILE%\My Documents\IISExpress\config\applicationhost.config`

Find a section that looks like this and add the second binding as shown below:
```xml
<site name="CommerceStarterKit.Web" id="3">
    <application path="/" applicationPool="Clr4IntegratedAppPool">
        <virtualDirectory path="/" physicalPath="...\src\web" />
    </application>
    <bindings>
        <binding protocol="http" bindingInformation="*:49883:localhost" />
        <!-- The B2B site -->
        <binding protocol="http" bindingInformation="*:49900:localhost" />
    </bindings>
</site>
```
You need to restart IIS Express after the change.

## How to Upgrade the Databases
Now and then the CMS and Commerce databases need to be upgraded. This can be done automatically by setting updateDatabaseSchema to true in web.config (`<episerver.framework updateDatabaseSchema="true">`), or it can be done using the `update-epidatabase` command in the Visual Studio Package Manager Console. 

The latest version of the code is synced with the downloadable databases. If you haven't changed any content (that you'd care to keep), just download new versions and replace the databases you've got.

If you want to update these databases yourself using the `update-epidatabase` command from the Package Manager Console, you need to change the connectionstring to a full path instead of the "magic" one using the `DataDirectory` setting. If you check the connectionstring, you can see that it points to the `DataDirectory` instead of the actual path to the database files on disk. During startup, asp.net will translate the `DataDirectory` string to the physical location of your site's `app_Data` directory. This is how LocalDB knows where to find the database by default.

	Data Source=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|CommerceStarterKit-Web.mdf; ... 

Since this is a Commerce project, we have two databases, and we want both Commerce Manager and the web site to use the same databases. Since both projects use `DataDirectory` in the connectionstring they would look into their respective `app_Data` directories, which of course wont work. We work around this by replacing the `DataDirectory` key ourselves during startup, and point it to a location outside the Web and Commerce Manager sites. This has to be done before any code tries to load the database, so for the web project, we have this in global.asax:

```c#
static WebGlobal()
{
    DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\db\");
    AppDomain.CurrentDomain.SetData("DataDirectory", dir.FullName);
}

``` 

In contrast, the `update-epidatabase` command will read the `connectionstrings.config` file directly and try to locate the databases in the `app_Data` directory below your web site, since this is where `DataDirectory` resolves by default. 

Change both AttachDbFilename settings:

	AttachDbFilename=c:\path\to\your\CommerceStarterKit-Web.mdf and
	AttachDbFilename=c:\path\to\your\CommerceStarterKit-CM.mdf

After you have run the `update-epidatabase` you can revert the connectionstring changes.

