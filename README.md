# TvScraper
## Future improvements
* A full suite of unit-tests
* Seperate the scraper and API into seperate applications
* The scraper could also be a microservice and the API merely consume the scraped data
* I would also use the 'Update' endpoints on the Maze API so we can ensure that the data is as up to date as possible when a shows cast changes
* My current approach is to scrape all shows using the Index endpoint, and then hit the "Cast" endpoint to import actors for each show. Once complete I thought there could be a better way to do this. Since my main limiting factor was the rate limiter.
* Another thought was that I could import all shows using the Index endpoint, and then all people using the people index endpoint. I could then use the person cast credits endpoint to create the linking table. This could potentially be faster, but when hitting the person cast credits endpoint we again would run into the rate limiting bottleneck 
* I would also like to combine code from the Show and Actor scrapers, they share some similarities, especially in error handling

## Notes
* I don't manually close the API connection to Maze between scrapes because RestSharp handles this for me. If I was using a WebClient, I would ensure that the connection was closed periodically to avoid getting my IP banned
