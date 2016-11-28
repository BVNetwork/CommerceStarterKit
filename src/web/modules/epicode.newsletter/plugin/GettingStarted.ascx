<%@ Control Language="C#" AutoEventWireup="true"
    CodeBehind="GettingStarted.ascx.cs" EnableViewState="false"
    Inherits="BVNetwork.EPiSendMail.Plugin.GettingStartedControl" %>

<div class="newsletter">
    <div class="container">
        <div class="row">
            <h1>Getting Started</h1>

            <p>
                You currently do not have any newsletters.
            </p>

        </div>

        <div class="row">
            <h2>Create a New Newsletter Page</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-CreateNewPage-400.png" width="400" class="img-responsive figure-left">
                <p>
                    The first thing you need to do is to create a page of the type Newsletter. This is the content you will send
                    as emails. The design and content of the Newsletter page should be created to work on most major email clients.
                </p>
                <ul class="listAction">
                    <li>Open Edit Mode</li>
                    <li>Select a location in the page tree</li>
                    <li>Add a new page of type Newsletter</li>
                </ul>
            </div>
        </div>
        <div class="row">
            <h2>Add Content to the Page</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-Editing-Newsletter-444.png" width="444" class="img-responsive figure-right">
                <p>
                    Add content to your newsletter, keep in mind avoiding content that will increase the risk of your newsletter beeing tagged as spam.
                </p>
                <p>
                    You should also pay attention to some of the non-visible properties that affects how your email will look, especially the <em>"From Address"</em> and
                    <em>"Email Subject"</em>. If you do not provide the <em>"Email Subject"</em>, the name of the page will be used for the newsletter subject when sent.
                </p>
                <p>
                    In the screenshot you can also see Mailgun specific properties that allows additional tracking from your Mailgun account.
                </p>
            </div>
        </div>
        <div class="row">
            <h2>See Newsletter Details</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-OpenDetails-205.png" width="205" class="img-responsive figure-left">
                To work with the Newsletter, open the Newsletter Details view from the <em>Select View</em> button on the page, and select the <em>Newsletter Details</em> item.
            </div>
        </div>
        <div class="row">
            <h2>Overview of the Newsletter Detail view</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-Newsletter-Details-500.png" width="500" class="img-responsive figure-right">
                <p>The details view consists of 4 distinct actions, and status of the sending process</p>
                <ol>
                    <li>Add Recipients</li>
                    <li>Remove Recipients</li>
                    <li>Test the Newsletter</li>
                    <li>Send the Newsletter</li>
                </ol>
            </div>
        </div>
        <div class="row">
            <h2>Add Recipients</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-Add-Recipients-400.png" width="400" class="img-responsive figure-left">
                <p>
                    There are several different ways of adding email addresses to the newsletter. The usual one is
                    to add all email addresses from a Recipient List to the newsletter. A Recipient List is a collection
                    of email addresses that you have collected previously, typically from sign-up forms on your web site.
                </p>
                <p>
                    You can also add email addresses manually by entering them in the text box that is shown if you click
                    the <em>Import From Text</em> link. You can paste many email addresses at a time, just separate them
                    with comma, semicolon or newlines. Any duplicates will be removed.
                </p>
            </div>
        </div>
        <div class="row">
            <h2>Clean The List</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-Clean-List-400.png" width="400" class="img-responsive figure-right">
                <p>
                    You should always clean the list of any email addresses that have opted out. Usually, these will be
                    removed from the recipient list in the first place, but if you usually import email addresses from
                    an external application, you risk adding people that have asked to be removed.
                </p>
                <p>
                    The Newsletter module uses Block lists for this, and you should always try to keep the block list
                    current. You can have several block lists. Email bounces (addresses that does not exists, or cannot
                    be delivered) should be added to the block list.
                </p>
                <p>
                    <strong>Important!</strong> If you do not clean your list of recipients regularly, you may be sending
                to a low quality mailing list, which will be punished by the receiving email servers, and you risk
                being black-listed.
                </p>
            </div>
        </div>


        <div class="row">
            <h2>Send a Test Newsletter</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-Send-Test-400.png" width="400" class="img-responsive figure-left">
                <p>
                    It is a good idea to send yourself a test version of the newsletter to verify that the content looks ok and all links works.
                </p>
                <p>
                    Add the email addresses that you want to test to the text box, and click the "Test Newsletter" button. This will send a test
                    version of the newsletter to the email addresses you specified with an appended <em>(Test)</em> to the email subject.
                </p>
                <p>Make sure you verify all links and that all images can be shown.</p>
            </div>
        </div>

        <div class="row">
            <h2>Send the Newsletter</h2>
        </div>
        <div class="row">
            <div class="col-sm-12 col-md-8">
                <img src="../content/images/userguide/UG-Ready-To-Send-500.png" width="500" class="img-responsive figure-right">
                <p>
                    When you're ready to send the newsletter, click the <em>Send Newsletter...</em> button. You will have to confirm that
                    you actually want to send it before it is sent.
                </p>
                <p>
                    <strong>Note!</strong> Depending on what sending method has been configured, it might take some time before the newsletter
                    is sent. Keep track of the status in the status panel to the right.
                </p>
            </div>
        </div>
        <div class="row">
            <p>
                For more information, <strong><a href="https://www.coderesort.com/p/epicode/wiki/Newsletter/UserGuide">see the online user guide</a></strong>
            </p>
        </div>
    </div>
</div>
