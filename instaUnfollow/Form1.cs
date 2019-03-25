using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace instaUnfollow
{
    //Create by chris campone
    //This tool is used to unfollow users that you follow on Instagram

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        ChromeDriver driver;
        string LoginBtnClass = "L3NKy";
        string profileIconXPath = "//*[@id=\"react-root\"]/section/nav/div[2]/div/div/div[3]/div/div[3]/a/span";
        string followModel = "isgrP";
        string modelUsernameClass = "_0imsa";
        string followingBtnClass = "BY3EC";

        bool forceStop = false;
        int amountUnFollowed = 0;


        private void Form1_Load(object sender, EventArgs e)
        {
            createDriver(false);

        }

        //creates the driver (chrome window)
        private void createDriver(bool headless)
        {
            try
            {
                //delete driver if one is open
                if (driver != null && driver.WindowHandles.Count > 0)
                {
                    driver.Quit();
                }
                ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                ChromeOptions options = new ChromeOptions();
                if (headless)
                {
                    //hide the broswer from sight
                    options.AddArgument("--headless");
                }
                //  options.EnableMobileEmulation("iPhone X");
                driver = new ChromeDriver(driverService, options);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //quit driver when closing form
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            driver.Quit();
        }

        //search webpages and wait for element to appear so load is completed
        async public Task search(string url, string xPathElementToWaitFor)
        {
            int trys = 0;
            driver.Navigate().GoToUrl(url);
            if (xPathElementToWaitFor.Length > 0)
            {

                await waitForElement(xPathElementToWaitFor);
            }
            else
            {
                if (trys >= 10)
                {
                    return;
                }
                trys++;
                await Task.Delay(1000);
            }
        }

        //method to wait for elements to appear
        async Task waitForElement(string xPath)
        {
            try
            {
                await Task.Delay(500);
                int trys = 0;
                int count = 0;
                do
                {
                    if (xPath.Contains("/"))
                    {
                        var links = driver.FindElementsByXPath(xPath);
                        count = links.Count;
                    }
                    else
                    {
                        var links = driver.FindElementsByClassName(xPath);
                        count = links.Count;
                    }

                    trys++;
                    await Task.Delay(1000);
                } while (count == 0 && trys < 10);
            }
            catch (Exception)
            {

            }
        }

        //checks for existing element
        private bool checkIfElementExists(string xPath)
        {
            try
            {
                if (xPath.Contains("/"))
                {
                    var links = driver.FindElementsByXPath(xPath);
                    if (links.Count > 0)
                    {
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var links = driver.FindElementsByClassName(xPath);
                    if (links.Count > 0)
                    {
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

        //add text to log
        private void cmd(string x)
        {
            string[] lines = textBox3.Text.Split(new Char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int count = lines.Length;

            if (count >= 17)
            {
                textBox3.Text = "";
                textBox3.Text += x + Environment.NewLine;
            }
            else
            {
                textBox3.Text += x + Environment.NewLine;
            }
        }

        //login method
        async Task loginInstagram()
        {

            try
            {
                if (textBox1.Text.Contains("@"))
                {
                    cmd("Please use your username to log in");
                    return;
                }
                button1.Enabled = false;
                await search("https://www.instagram.com/accounts/login/", LoginBtnClass);
                if (driver.Url.Equals("https://www.instagram.com/"))
                {
                    cmd("You are already logged in");
                    return;
                }
                if (checkIfElementExists("KPZNL"))
                {
                    driver.FindElementByClassName("KPZNL").Click();
                    await Task.Delay(250);
                }
                cmd("Logging into instagram");
                driver.FindElement(By.Name("username")).SendKeys(textBox1.Text);
                driver.FindElement(By.Name("password")).SendKeys(textBox2.Text);
                await Task.Delay(500);
                driver.FindElement(By.ClassName(LoginBtnClass)).Click();
                await waitForElement(profileIconXPath);
                if (checkIfElementExists("_3m3RQ"))
                {
                    driver.FindElementByClassName("_7XMpj").Click();
                    await Task.Delay(350);
                }
                else if (checkIfElementExists("O4QwN"))
                {
                    if (driver.FindElementByClassName("O4QwN").Text.Contains("Suspicious Login Attempt"))
                    {
                        button1.Enabled = true;
                        textBox2.Text = "";
                        cmd("Suspicious Login Attempt, Please Fix");
                        return;
                    }
                }
                if (checkIfElementExists(profileIconXPath))
                {
                    //check for pop up
                    if (checkIfElementExists("/html/body/div[2]/div/div/div"))
                    {
                        driver.FindElementByClassName("HoLwm").Click();
                    }
                    cmd("Logged In");
                    button1.Enabled = false;
                    button1.Text = "Logged In";
                }
                else
                {
                    button1.Enabled = true;
                    textBox2.Text = "";
                    if (driver.Url.ToLower().Contains("challenge"))
                    {
                        cmd("Your account is locked with a challenge. Please fix this before using the bot");
                    }
                    else
                    {
                        cmd("Wrong credentials?");
                    }

                }
            }
            catch (Exception ex)
            {
               

                //MessageBox.Show(ex.ToString());
            }

        }

        //login btn
        async private void button1_Click(object sender, EventArgs e)
        {
            await loginInstagram();
        }

        //unfollow users
        async private void button2_Click(object sender, EventArgs e)
        {
            await unfollowUsernames();
        }

        //unfollows users not following you
        async private Task unfollowUsernames()
        {
            try
            {
                
                do
                {
                    ArrayList usersToUnfollow = new ArrayList();
                    await search("https://www.instagram.com/" + textBox1.Text + "/", profileIconXPath);
                    var links = driver.FindElementsByClassName("-nal3"); //gets the links followers,following and pictureCount
                    cmd("Gathering Users to unfollow");
                    foreach (IWebElement item in links)
                    {
                        //click on following
                        if (item.Text.Contains("following"))
                        {
                            item.Click();
                            await Task.Delay(1500);
                            break;
                        }
                    }
                    await waitForElement(followModel);
                    driver.FindElement(By.ClassName(followModel)).Click();//clicks on model to page down
                    await Task.Delay(1000);
                    for (int i = 0; i < 10; i++)
                    {
                        driver.FindElement(By.ClassName(followModel)).Click();//clicks on model to page down
                        new Actions(driver).SendKeys(OpenQA.Selenium.Keys.PageDown).Perform();
                        await Task.Delay(50);
                        if (i < 3)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    foreach (var item in driver.FindElements(By.ClassName(modelUsernameClass)))//class to get username of user
                    {
                        if (item.Text.Contains("Verified"))
                        {
                            usersToUnfollow.Add(item.Text.Substring(0, item.Text.IndexOf("Verified")));
                        }
                        else
                        {
                            usersToUnfollow.Add(item.Text);
                        }

                    }
                    string user = "";
                    if (usersToUnfollow.Count == 0)
                    {
                        cmd("You have no users to unfollow");
                        return;
                    }
                    cmd("Gathered: " + usersToUnfollow.Count);
                    foreach (string item in usersToUnfollow)
                    {
                        if(forceStop){
                            forceStop = false;
                            return;
                        }
                        //unfollow each user
                        user = item;
                        await search("https://www.instagram.com/" + user + "/", followingBtnClass);
                        if ((driver.PageSource.Contains("\"follows_viewer\":false")) && !driver.PageSource.Contains("Sorry, this page isn't available"))
                        {
                            var btns = driver.FindElementsByClassName(followingBtnClass);
                            foreach (IWebElement btn in btns)
                            {
                                if (driver.PageSource.Contains("\"followed_by_viewer\":true"))
                                {
                                    btn.Click();
                                    await Task.Delay(1000);
                                    if (checkIfElementExists("pbNvD"))//pop up unfollow
                                    {
                                        var unfollowBtns = driver.FindElementsByClassName("aOOlW");
                                        foreach (IWebElement itemBtn in unfollowBtns)
                                        {
                                            if (itemBtn.Text.Contains("Unfollow"))
                                            {
                                                itemBtn.Click();
                                                break;
                                            }
                                        }
                                        for (int i = 0; i < 10; i++)
                                        {
                                            if (!driver.FindElementByClassName(followingBtnClass).Text.Equals("Follow"))
                                            {
                                                await Task.Delay(300);
                                            }
                                            else
                                            {
                                                break;
                                            }

                                        }
                                        await search("https://www.instagram.com/" + user + "/", followingBtnClass);
                                        if (driver.PageSource.Contains("\"followed_by_viewer\":false"))
                                        {
                                            amountUnFollowed++;
                                            cmd("Un-Followed: " + user);
                                            label4.Text = amountUnFollowed.ToString();
                                        }
                                        else if (driver.PageSource.Contains("\"followed_by_viewer\":true"))
                                        {
                                            cmd("Temp Un-Follow blocked");
                                            for (int i = 180; i >0; i--)
                                            {
                                                cmd("Spam Wait: "+i.ToString());
                                                await Task.Delay(1000);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        await search("https://www.instagram.com/" + user + "/", "_5f5mN");
                                        if (driver.PageSource.Contains("\"followed_by_viewer\":false"))
                                        {
                                            amountUnFollowed++;
                                            cmd("Un-Followed: " + user);
                                            label4.Text = amountUnFollowed.ToString();
                                        }
                                        else if (driver.PageSource.Contains("\"followed_by_viewer\":true"))
                                        {
                                            cmd("Temp Un-Follow blocked");
                                            for (int i = 180; i > 0; i--)
                                            {
                                                cmd("Spam Wait: " + i.ToString());
                                                await Task.Delay(1000);
                                            }
                                        }
                                    }

                                }
                                
                            }
                        }
                        else
                        {
                            cmd("User is following you: " + user);
                        }
                    }//done unfollowing group 

                } while (!forceStop);
                forceStop = false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Unfollowing: "+ex.ToString());
                // screenshot("error");
            }
        }

        //stop btn
        private void button3_Click(object sender, EventArgs e)
        {
            forceStop = true;
        } 


    }
}
