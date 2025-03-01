const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

var config = {
    entry: './src/index.tsx',
    output: {
        path: path.resolve(__dirname, '../wwwroot'),
        filename: 'bundle.js',
        publicPath: '/',
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader'],
            },
            {
                test: /\.(png|jpg|gif|svg)$/,
                use: ['file-loader'],
            },
        ],
    },
    plugins: [
        new CleanWebpackPlugin(),
        new HtmlWebpackPlugin({
            template: './public/index.html',
            favicon: './public/favicon-96x96.png'
        }),
    ],
    devServer: {
        static: path.resolve(__dirname, 'public'),
        historyApiFallback: true, // Enables React Router
        /*proxy: [
            {
                context: ['/api'], // Proxy API requests
                target: 'http://localhost:5000',
                changeOrigin: true,
                secure: false,
            },
        ],*/
        port: 3000,
        open: true,
    },
    // mode: 'development',
};

module.exports = (env, argv) => {

    console.log('argv:', argv);

    if (argv.mode === 'development') {
        config.devtool = 'source-map';
        // config.watch = true;
    }

    if (argv.mode === 'production') {
        // config.mode = 'production';
    }

    return config;
};
