// @ts-check

var path = require("path");
var webpack = require("webpack");
const VueLoader = require("vue-loader/lib/plugin");
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const OptimizeCssnanoPlugin = require("@intervolga/optimize-cssnano-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

// This helper function is not strictly necessary.
// I just don't like repeating the path.join a dozen times.
function srcPath(subdir) {
    return path.join(__dirname, "src", subdir);
}

module.exports = {
    mode: "development",
    entry: ["./src/main.ts"],
    output: {
        path: path.resolve(__dirname, "./dist"),
        publicPath: "/",
        filename: "bundle.js"
    },
    module: {
        rules: [

            {
                test: /\.scss$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader'
                    },
                    {
                        loader: 'sass-loader',
                        options: {
                            sourceMap: true,
                            // options...
                        }
                    }
                ]
            },
            // Required for bootstrap to function correctly
            {
                test: /bootstrap.+\.(jsx|js)$/,
                loader: "imports-loader?jQuery=jquery,$=jquery,this=>window"
            },
            // Required to load files referenced from bootstrap css
            {
                test: /\.(png|woff|woff2|eot|ttf|svg|ico)$/,
                loader: "url-loader?limit=100000"
            },
            {
                test: /\.less$/,
                use: [
                    'vue-style-loader',
                    'css-loader',
                    'less-loader',
                    'sass-loader'
                ]
            },
            // Required to load any css files
            {
                test: /\.css$/,
                use: [
                    process.env.NODE_ENV !== "production"
                        ? "vue-style-loader"
                        : MiniCssExtractPlugin.loader,
                    "css-loader"
                ]
            },
            // Required for writing less in .vue files
            {
                test: /\.less$/,
                use: [
                    "vue-style-loader",
                    "css-loader",
                    "less-loader"
                ]
            },
            {
                test: /\.sass$/,
                use: [
                    "vue-style-loader",
                    "css-loader",
                    "sass-loader"
                ]
            },
            // Required for importing .vue files
            {
                test: /\.vue$/,
                loader: "vue-loader",
                options: {
                    loaders: {
                        // Since sass-loader (weirdly) has SCSS as its default parse mode, we map
                        // the "scss" and "sass" values for the lang attribute to the right configs here.
                        // other preprocessors should work out of the box, no loader config like this necessary.
                        'scss': "vue-style-loader!css-loader!sass-loader",
                        'sass': "vue-style-loader!css-loader!sass-loader?indentedSyntax"
                    }
                    // other vue-loader options go here
                }
            },
            // required for treating .vue files as typescript
            {
                test: /\.tsx?$/,
                loader: "ts-loader",
                exclude: /node_modules|config.ts/,
                options: {
                    appendTsSuffixTo: [/\.vue$/]
                }
            },
            // required to load images
            {
                test: /\.(png|jpg|gif|svg|ico)$/,
                loader: "file-loader",
                options: {
                    name: "[name].[ext]?[hash]"
                }
            }
        ]
    },
    resolve: {
        extensions: [".ts", ".js", ".vue", ".json"],
        alias: {
            'node_modules': path.join(__dirname, "node_modules"),
            'vue$': "vue/dist/vue.esm.js",
            api: srcPath("api"),
            components: srcPath("components"),
            typings: srcPath("typings")
        }
    },
    devServer: {
        historyApiFallback: true,
        noInfo: true
    },
    performance: {
        hints: false
    },
    devtool: "#eval-source-map", // Enable this to turn on source maps, should not be included in production builds!
    optimization: {
        minimize: true,
        minimizer: [
            new UglifyJsPlugin({
                uglifyOptions: {
                    output: {
                        comments: false
                    }
                }
            })
        ]
    },
    plugins: [
        // make sure to include the plugin for the magic
        new VueLoader(),
        new MiniCssExtractPlugin({
            // Options similar to the same options in webpackOptions.output
            // both options are optional
            filename: "style.css"
        })
    ]
};

if (process.env.NODE_ENV === "production") {
    module.exports.devtool = false;
    // http://vue-loader.vuejs.org/en/workflow/production.html
    module.exports.plugins = (module.exports.plugins || []).concat([
        new webpack.DefinePlugin({
            'process.env': {
                NODE_ENV: '"production"'
            }
        }),
        new OptimizeCssnanoPlugin({
            cssnanoOptions: {
                preset: ["default", {
                    discardComments: {
                        removeAll: true
                    }
                }]
            }
        })
    ]);
}