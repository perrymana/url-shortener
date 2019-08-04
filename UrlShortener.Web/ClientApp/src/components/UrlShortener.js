﻿import React from 'react'
import PropTypes from 'prop-types'

export default class UrlShortener extends React.Component {
    constructor(props) {
        super(props);
        this.state = { longUrl: '', shortenedUrl: null, error: '' };

        this.handleChange = this.handleChange.bind(this);
        this.createShortUrl = this.createShortUrl.bind(this);
    }

    handleChange(evt) {
        // check it out: we get the evt.target.name (which will be either "email" or "password")
        // and use it to target the key on our `state` object with the same name, using bracket syntax
        this.setState({ [evt.target.name]: evt.target.value });
    }

    async createShortUrl(event) {
        event.preventDefault();

        this.setState({ shortenedUrl: shortenedUrl, error: '' });

        // Add "http://" prefix if it hasn't been specified.
        var url = this.state.longUrl;
        if (!url.toLowerCase().startsWith("http://") && !url.toLowerCase().startsWith("https://"))
        {
            url = "http://" + url;
        }

        try {
            let response = await fetch('api/ShortenedUrls', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    longUrl: url
                })
            })

            if (response.status >= 200 && response.status < 300) {
                var shortenedUrl = await response.json();

                this.setState({ shortenedUrl: shortenedUrl, error: '' });
            }
            else if (response.status === 400) {
                this.setState({ shortenedUrl: null, error: "Invalid Url" });
            } else {
                this.setState({ shortenedUrl: null, error: "Something errored" });
                console.warn(response);
            }
        } catch (error) {
            console.error(error);
        }
    }

    render() {

        let shortUrlDiv;
        let errorDiv;

        if (this.state.shortenedUrl) {
            shortUrlDiv = (
                <div>
                    <h3>Short Url Created</h3>
                    <p>Your new shortened url is: <a href={this.state.shortenedUrl.shortUrl} target="_blank">{this.state.shortenedUrl.shortUrl}</a></p>
                    <p>This will redirect to: {this.state.shortenedUrl.longUrl}</p>
                </div>
            );
        }

        return (
            <div>
                <h3>Create new short url</h3>
                <form onSubmit={this.createShortUrl}>
                    <div class="form-group">
                        <label for="longUrl">
                            Enter a long URL to make smaller:
                        </label>
                        <input type="text" class="form-control" name="longUrl" placeholder="enter long url" value={this.state.longUrl} onChange={this.handleChange} />
                        <span>{this.state.error}</span>
                    </div>
                    <div class="form-group">
                        <input type="submit" class="btn btn-primary" value="Smallify!" />
                    </div>
                </form>
                {shortUrlDiv}
                {errorDiv}
            </div>
        );
    }
}

